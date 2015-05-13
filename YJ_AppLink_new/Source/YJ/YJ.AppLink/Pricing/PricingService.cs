using System;
using System.Collections;

using System.Threading;

using YJ.AppLink.Api;
using YJ.AppLink;

namespace YJ.AppLink.Pricing
{
	public delegate void PriceUpdateRequestedHandler (object sender, PriceUpdateRequestedEventArgs args);
	public delegate void MarketRemovedHandler (object sender, MarketRemovedEventArgs args);

    /// <summary>
    /// PricingService provides methods and events for configuring, 
    /// sending and receiving PriceUpdate and PriceResponse messages with 
    /// YJ Energy.
    /// </summary>
	public class PricingService : Service
	{	
        /// <summary>
        /// Occurs when a PriceUpdate Price message is received from YJ Energy, requesting a price for a give market
        /// </summary>
		public event PriceUpdateRequestedHandler PriceUpdateRequested;
		
        /// <summary>
        /// Occurs when a PriceUpdate Remove message is received from YJ Energy, indicating a market no longer
        /// needs to be priced
        /// </summary>
        public event MarketRemovedHandler MarketRemoved;

        public const string PRICE_ACTION = "Price";
        public const string REMOVE_ACTION = "Remove";

        /// <summary>
        /// The current version of the API that the service supports.
        /// </summary>
        public const string VERSION = "Equities 1.6";

        protected Hashtable allMkts = new Hashtable();
		protected Hashtable filteredMkts = new Hashtable();

		protected IMarketFilter filter = null;

        internal PricingService(Session session) : base(ServiceName.Pricing, PricingService.VERSION, session) { }

		#region public methods

		/// <summary>
		/// Gets an array of all MarketData objects the AppLink has received
		/// </summary>
        public MarketData[] GetAllMarkets()
		{
			Hashtable clone;
			lock (allMkts)
			{
				clone = this.allMkts.Clone() as Hashtable;
			}
 
			if (clone != null)
			{
				MarketData[] mkts = new MarketData[clone.Values.Count];
				clone.Values.CopyTo (mkts, 0);
				return mkts;
			}
			else
			{
				return new MarketData[0];
			}
		}

        /// <summary>
        /// Gets an array of MarketData objects that passed a specified filter
        /// </summary>
		public MarketData[] GetFilteredMarkets()
		{
			Hashtable clone;
			lock (filteredMkts)
			{
				clone = this.filteredMkts.Clone() as Hashtable;
			}
 
			if (clone != null)
			{
				MarketData[] mkts = new MarketData[clone.Values.Count];
				clone.Values.CopyTo (mkts, 0);
				return mkts;
			}
			else
			{
				return new MarketData[0];
			}
		}

        /// <summary>
        /// Clears the stored markets
        /// </summary>
		public void ClearMarkets()
		{
			lock (filteredMkts)
			{
				this.filteredMkts.Clear();
			}
			
			lock (allMkts)
			{
				this.allMkts.Clear();
			}
		}

        /// <summary>
        /// Gets a MarketData object based on its unique id.  Return null if one was not found.
        /// </summary>
		public MarketData GetMarketData (string id)
		{
			if (id == "" ||
				id == null)
			{
				Session.Logger.Warn("GetMarketData invalid id", this);
				return null;
			}
			
			MarketData mkt = null;
			lock (allMkts)
			{
				if (id != null && 
					allMkts.Contains(id))
				{
					mkt = allMkts[id] as MarketData;
				}
			}

            if (mkt == null)
            {
                Session.Logger.Warn("MarketData: " + id + " not found", this);
            }

			return mkt;
		}

        /// <summary>
        /// Gets an array of MarketData object that pass a given filter
        /// </summary>
		public MarketData[] GetSelectedMarkets (IMarketFilter selectionFilter)
		{
			Session.Logger.Debug("GetSelectedMarkets", this);
			if (selectionFilter != null)
			{
				MarketData[] mkts = GetAllMarkets();
				ArrayList selectedMarkets = new ArrayList();
				foreach (MarketData mkt in mkts)
				{
					try
					{
						if (selectionFilter.PassesFilter(mkt))
						{
							selectedMarkets.Add(mkt);
						}
					}
					catch (Exception e)
					{
						Session.OnSessionError("Failure testing a selected market", this, e);
					}
				}

				MarketData[] ret = new MarketData[selectedMarkets.Count];
				selectedMarkets.CopyTo(ret);
				return ret;
			}
			else
			{
				return new MarketData[0];
			}
		}

        /// <summary>
        /// Sets a filter on the pricing service to ignore PriceRequests with MarketData that do not pass the filter.
        /// </summary>
		public void SetMarketFilter(IMarketFilter filter)
		{
			this.filter = filter;
			ResetFilteredMarkets();
		}

        /// <summary>
        /// Clears the current filter.
        /// </summary>
		public void RemoveFilter()
		{
			this.filter = null;
			ResetFilteredMarkets();
		}

        /// <summary>
        /// Updates the collection of currently stored filtered markets based on the current filter
        /// </summary>
		public void ResetFilteredMarkets()
		{
			MarketData[] mkts = this.GetAllMarkets();
			lock (this.filteredMkts)
			{
				this.filteredMkts.Clear();
				foreach (MarketData mkt in mkts)
				{
					try
					{
						if (PassesFilter(mkt))
						{
							filteredMkts.Add(mkt.Id, mkt);
						}
					}
					catch (Exception e)
					{
						Session.OnSessionError("Error resetting filtered markets", this, e);
					}
				}
			}
		}

		#endregion
		
		#region internal methods

		internal void SendPriceResponse(YJ.AppLink.Api.PriceResponse resp)
		{
			try
			{
				Session.Logger.Info("Sending price response", this);
				Sender.Send(resp);
			}
			catch (Exception e)
			{
				Session.OnSessionError("Failure sending price response", this , e);
				throw e;
			}
		}

		#endregion

		#region protected methods

		protected override void HandleReceivedMessage(object sender, YJ.AppLink.Net.MessageReceivedEventArgs args)
		{
			// note: if the session does not have a threadSafeGUIControl 
			// this is invoked asynchronously and is not thread safe.
			
			try
			{
				Session.Logger.Debug("Message received", this);
				if (args.Envelope.Content != null &&
					args.Envelope.Content is PriceUpdate)
				{
					PriceUpdate update = (PriceUpdate) args.Envelope.Content;
					HandlePriceUpdate(update);
				}

			}
			catch (Exception e)
			{
				Session.OnSessionError("Failure handling received message", this, e);
				// do not throw exception
			}
		}

		protected void HandlePriceUpdate(PriceUpdate update)
		{
			try
			{
				Session.Logger.Info(string.Format("PriceUpdate received from {0}", update.User), this);
				switch (update.Action)
				{
					case PricingService.PRICE_ACTION:
						HandlePriceMarketAction(update.MarketDetail, update.User);
						break;
					case PricingService.REMOVE_ACTION:
						HandleRemoveMarketAction(update.MarketDetailId, update.User);
						break;
					default:
						Session.Logger.Warn("Unhandled PriceUpdate action received: " + update.Action, this);
						break;
				}
			}
			catch (Exception e)
			{
				Session.OnSessionError("Failure handling PriceUpdate message", this, e);
				// do not throw exception
			}
		}

		protected void HandleRemoveMarketAction(string mktId, string userName)
		{
			Session.Logger.Info("Request to remove markets received", this);
            try
            {
                lock (filteredMkts)
                {
                    if (filteredMkts.Contains(mktId))
                        filteredMkts.Remove(mktId);
                }

                lock (allMkts)
                {
                    if (allMkts.Contains(mktId))
                        allMkts.Remove(mktId);
                }

                OnMarketRemoved(mktId, userName);
            }
            catch (Exception e)
            {
                Session.OnSessionError("Failure handling request to remove market", this, e);
                // do not throw exception
            }
		}

		protected void OnMarketRemoved(string mktId, string userName)
		{
			try
			{
				if (MarketRemoved != null)
				{
					MarketRemovedEventArgs args = new MarketRemovedEventArgs(mktId);
                    args.UserName = userName;
					MarketRemoved(this, args);
				}
			}
			catch (Exception e)
			{
				Session.OnSessionError("Failure raising MarketRemoved event", this, e);
				throw e;
			}
		}
        //array of mkt
		protected void HandlePriceMarketAction (MarketDetail md, string userName)
		{
			Session.Logger.Info("Request to price markets received", this);
            try
            {
                MarketData mkt = new MarketData(md, this, Session);
                lock (allMkts)
                {
                    allMkts[mkt.Id] = mkt;
                }

                if (PassesFilter(mkt))
                {
                    Session.Logger.Debug("Request to price market passed filters", this);
                    lock (this.filteredMkts)
                    {
                        this.filteredMkts[mkt.Id] = mkt;
                    }
                    OnPriceUpdateRequested(mkt, userName);
                }
                else
                {
                    Session.Logger.Info("Request to price market did not pass filters: " + mkt.Id, this);
                }
            }
            catch (Exception e)
            {
                Session.OnSessionError("Failure handling request to price market", this, e);
                // do not throw exception
            }
		}

		protected bool PassesFilter(MarketData mkt)
		{			
			try
			{
				if (filter != null)
				{
					return filter.PassesFilter(mkt);
				}
				return true;
			}
			catch (Exception e)
			{
				Session.OnSessionError("Failure filtering market", this, e);
				throw e;
			}
		}

		protected void OnPriceUpdateRequested(MarketData mkt, string userName)
		{
			try
			{
				if (PriceUpdateRequested != null)
				{
					PriceUpdateRequestedEventArgs args = new PriceUpdateRequestedEventArgs();
					args.Market = mkt;
                    args.UserName = userName;

					PriceUpdateRequested(this, args);
				}
			}
			catch (Exception e)
			{
				Session.OnSessionError("Failure raising PriceUpdateRequested event", this, e);
				throw e;
			}
		}

		#endregion

		/*
		 * Note: This method could be enabled if you want
		 * to clear all mkts when the connection is closed
		 * or the service is stoppped
		internal override void Stop()
		{
			base.Stop ();
			this.ClearMarkets();
		}
		*/

	}

	public class PriceUpdateRequestedEventArgs : System.EventArgs
	{
		private MarketData mkt;
        private string userName;
		
		public PriceUpdateRequestedEventArgs():base() {}
		
		public MarketData Market
		{
			get { return mkt; }
			set { this.mkt = value; }
		}

        public string UserName
        {
            get { return userName; }
            set { this.userName = value; }
        }

	}

	public class MarketRemovedEventArgs : System.EventArgs
	{
		private string mktId;
        private string userName;
		
		public MarketRemovedEventArgs(string mktId):base() 
		{
			this.mktId = mktId;	
		}
		
		public string MarketId
		{
			get { return mktId; }
		}

        public string UserName
        {
            get { return userName; }
            set { this.userName = value; }
        }

	}
}
