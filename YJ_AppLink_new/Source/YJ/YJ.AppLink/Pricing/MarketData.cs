using System;
using System.Collections;

using YJ.AppLink.Api;
using YJ.AppLink;

namespace YJ.AppLink.Pricing
{
	/// <summary>
	/// MarketData provides information about a market for pricing, 
    /// including a breakdown of the option and stock legs of the market, 
    /// and methods to send PriceResponse messages.
	/// </summary>
    public class MarketData
	{
		private MarketDetail mkt;
        private PricingService pricingService;
        private Session session;

        private QuoteData bestAskQuoteData;
        private QuoteData bestBidQuoteData;
        private QuoteData lastQuoteData;

        private double theoreticalOverride = Double.NaN;
		private bool theoreticalOverrideSpecified = false;
		private double volScalingNumberOverride = Double.NaN;
		private bool volScalingNumberOverrideSpecified = false;
        private DateTime lastPricedTime = DateTime.MinValue;

        
        /* NOTE: The following fields are declared public for VBA/com interop. 
         * If they are private and exposed as Properties,
         * VBA cannot understand the complex objects in the array.
         */

        /// <summary>
        /// An array of Option legs for the market
        /// </summary>
        public Option[] Options = null;

        /// <summary>
        /// An array of stock legs for the market
        /// </summary>
        public Stock[] Stocks = null;

        /// <summary>
        /// An array of swap legs for the market
        /// </summary>
        public Swap[] Swaps = null;

        // may need a public KeyValue[] for VBA access
        private ArrayList responseKeyValues = null;


		internal MarketData(MarketDetail mkt, PricingService pricingService, Session session)
		{
			try
			{
                Options = new Option[0];
                Stocks = new Stock[0];
                Swaps = new Swap[0];

				this.mkt = mkt;
				this.pricingService = pricingService;
				this.session = session;

                if (mkt.BestAsk != null)
                    bestAskQuoteData = new QuoteData(mkt.BestAsk);

                if (mkt.BestBid != null)
                    bestBidQuoteData = new QuoteData(mkt.BestBid);

                if (mkt.LastQuote != null)
                    lastQuoteData = new QuoteData(mkt.LastQuote);

				ArrayList olegs = new ArrayList();
				ArrayList slegs = new ArrayList();
                ArrayList swaplegs = new ArrayList();

				if (mkt.OptionLegs != null &&
					mkt.OptionLegs.Length > 0)
				{
					foreach (OptionLeg leg in mkt.OptionLegs)
					{
                        olegs.Add(new Option(leg, this, session));
					}
				}

				if (mkt.StockLegs != null &&
					mkt.StockLegs.Length > 0)
				{
					foreach (StockLeg leg in mkt.StockLegs)
					{
                        slegs.Add(new Stock(leg, this, session));
					}
				}

                if (mkt.SwapLegs != null &&
                    mkt.SwapLegs.Length > 0)
                {
                    foreach (SwapLeg leg in mkt.SwapLegs)
                    {
                        swaplegs.Add(new Swap(leg, this, session));
                    }
                }

				if (olegs.Count > 0)
				{
					Options = new Option[olegs.Count];
					olegs.CopyTo(Options);
				}

				if (slegs.Count > 0)
				{
                    Stocks = new Stock[slegs.Count];
                    slegs.CopyTo(Stocks);
				}

                if (swaplegs.Count > 0)
                {
                    Swaps = new Swap[swaplegs.Count];
                    swaplegs.CopyTo(Swaps);
                }

				if (Options.Length > 0 &&
                    Stocks.Length > 0)
				{
					foreach (Option o in Options)
					{
                        foreach (Stock s in Stocks)
						{
							if (s.CrossId == o.CrossId)
							{
								o.SetCross(s);
								break;
							}
						}
					}
				}

			}
			catch (Exception e)
			{
				session.OnSessionError("Failure creating MarketData instance", this, e);
				throw e;
			}
		}
		
		
		#region public properties

		/// <summary>
		/// Gets the unique market Id
		/// </summary>
        public string Id
		{
			get { return mkt.Id; }
		}

        /// <summary>
        /// Gets a string representation of the market
        /// </summary>
		public string Description
		{
			get 
            {
                //if (Options != null && Options.Length > 0)
                //{
                //    return Options[0].StockSymbol + " " + mkt.Description;
                //}

                return mkt.Description; 
            }
		}

        /// <summary>
        /// Gets the type of strategy the market represents, i.e. straddle
        /// </summary>
        public string Strategy
        {
            get { return mkt.Strategy; }
        }

        /// <summary>
        /// Gets a DateTime value for the last time the market price was updated by the AppLink
        /// </summary>
		public DateTime LastPricedTime
		{
			get { return this.lastPricedTime; }
		}

        /// <summary>
        /// Gets if details for the last trade are available
        /// </summary>
        public bool LastTradeSpecified
        {
            get { return mkt.LastTradeSpecified; }
        }

        /// <summary>
        /// Gets the last trade value of the market provided by YJ Energy.  Returns double.NaN if not available.
        /// </summary>
        public double LastTrade
        {
            get
            {
                if (LastTradeSpecified)
                {
                    return mkt.LastTrade;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        /// <summary>
        /// Gets the last trade DateTime of the market provided by YJ Energy.  Returns 1/1/1 if not available.
        /// </summary>
        public DateTime LastTradeTime
        {
            get { return mkt.LastTradeTime; }
        }


        public bool BestBidSpecified
        {
            get
            {
                return mkt.BestBid != null && mkt.BestBid.BidSpecified;
            }
        }

        /// <summary>
        /// Gets the best bid value of the market provided by YJ Energy.  Returns double.NaN if not available.
        /// </summary>
        public double BestBid
        {
            get
            {
                if (BestBidSpecified)
                {
                    return mkt.BestBid.Bid;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        public QuoteData BestBidQuote
        {
            get { return this.bestBidQuoteData; }
        }

        /// <summary>
        /// Gets the best bid's DateTime of the market provided by YJ Energy.  Returns DateTime.MinValue if not available.
        /// </summary>
        public DateTime BestBidTime
        {
            get 
            {
                if (mkt.BestBid != null &&
                    mkt.BestBid.QuoteTimeSpecified)
                {
                    return mkt.BestBid.QuoteTime;
                }

                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the sender name of the current best bid.  Returns "" if not available.
        /// </summary>
        public string BestBidSender
        {
            get
            {
                if (mkt.BestBid != null &&
                    mkt.BestBid.Sender != null)
                    return mkt.BestBid.Sender.Name;

                return "";

            }
        }

        public bool BestAskSpecified
        {
            get
            {
                return mkt.BestAsk != null && mkt.BestAsk.AskSpecified; 
            }
        }

        /// <summary>
        /// Gets the best ask value of the market provided by YJ Energy.  Returns double.NaN if not available.
        /// </summary>
        public double BestAsk
        {
            get
            {
                if (BestAskSpecified)
                {
                    return mkt.BestAsk.Ask;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        public QuoteData BestAskQuote
        {
            get { return this.bestAskQuoteData; }
        }

        /// <summary>
        /// Gets the best ask's DateTime of the market provided by YJ Energy.  Returns DateTime.MinValue if not available.
        /// </summary>
        public DateTime BestAskTime
        {
            get
            {
                if (mkt.BestAsk != null &&
                    mkt.BestAsk.QuoteTimeSpecified)
                {
                    return mkt.BestAsk.QuoteTime;
                }

                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the sender name of the current best ask.  Returns "" if not available.
        /// </summary>
        public string BestAskSender
        {
            get
            {
                if (mkt.BestAsk != null &&
                    mkt.BestAsk.Sender != null)
                    return mkt.BestAsk.Sender.Name;

                return "";

            }
        }

        public QuoteData LastQuote
        {
            get { return this.lastQuoteData; }
        }

        public bool LastQuoteAskSpecified
        {
            get
            {
                return mkt.LastQuote != null && mkt.LastQuote.AskSpecified;
            }
        }

        /// <summary>
        /// Gets the last ask value of the market provided by YJ Energy.  Returns double.NaN if not available.
        /// </summary>
        public double LastQuoteAsk
        {
            get
            {
                if (LastQuoteAskSpecified)
                {
                    return mkt.LastQuote.Ask;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        public bool LastQuoteBidSpecified
        {
            get
            {
                return mkt.LastQuote != null && mkt.LastQuote.BidSpecified;
            }
        }

        /// <summary>
        /// Gets the last bid value of the market provided by YJ Energy.  Returns double.NaN if not available.
        /// </summary>
        public double LastQuoteBid
        {
            get
            {
                if (LastQuoteBidSpecified)
                {
                    return mkt.LastQuote.Bid;
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        /// <summary>
        /// Gets the last quote's DateTime of the market provided by YJ Energy.  Returns DateTime.MinValue if not available.
        /// </summary>
        public DateTime LastQuoteTime
        {
            get
            {
                if (mkt.LastQuote != null &&
                    mkt.LastQuote.QuoteTimeSpecified)
                {
                    return mkt.LastQuote.QuoteTime;
                }

                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the sender name of the current best ask.  Returns "" if not available.
        /// </summary>
        public string LastQuoteSender
        {
            get
            {
                if (mkt.LastQuote != null &&
                    mkt.LastQuote.Sender != null)
                    return mkt.LastQuote.Sender.Name;

                return "";

            }
        }


		#endregion

		#region public methods
		
		public override string ToString()
		{
			return Description;
		}

        /// <summary>
        /// Adds a key/value pair to a market, along with foreground and background cell formatting information.
        /// Specify System.Drawing.Color.Empty to clear to omit a foreground or background color.
        /// 
        /// Note: this method adds the key/value pair to the market's PriceResponse, but does not send the response message
        /// </summary>
        public void AddOpenAPIKeyValue(string key, string value, 
            System.Drawing.Color foreground, System.Drawing.Color background)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            sb.Append(string.Format("value={0}", value));

            if (foreground != System.Drawing.Color.Empty)
            {
                string fHex = System.Drawing.ColorTranslator.ToHtml(foreground);
                sb.Append(string.Format(",foreground={0}", fHex));
            }

            if (background != System.Drawing.Color.Empty)
            {
                string bHex = System.Drawing.ColorTranslator.ToHtml(background);
                sb.Append(string.Format(",background={0}", bHex));
            }

            AddOpenAPIKeyValue(key, sb.ToString());
        }


        /// <summary>
        /// Adds a key/value pair to a market, without formatting information
        /// 
        /// Note: this method adds the key/value pair to the market's PriceResponse, but does not send the response message
        /// </summary>
        public void AddOpenAPIKeyValue(string key, string value)
        {
            if (responseKeyValues == null)
                responseKeyValues = new ArrayList();

            foreach (KeyValue k in responseKeyValues)
            {
                if (k.Key.Equals(key))
                {
                    k.Value = value;
                    return;
                }
            }
            
            KeyValue kv = new KeyValue();
            kv.Key = key;
            kv.Value = value;

            responseKeyValues.Add(kv);
        }

        /// <summary>
        /// This method builds and sends a PriceResponse message with only Open API key/value pairs, 
        /// it does not send or update theoretical price information.  
        /// 
         /// </summary>
        public void SendOpenApiValues()
        {
            if (responseKeyValues == null)
                return;
            
            session.Logger.Info("Sending price response", this);
            PriceResponse resp = new YJ.AppLink.Api.PriceResponse();
            resp.MarketDetailId = Id;

            resp.KeyValues = new KeyValue[responseKeyValues.Count];
            responseKeyValues.CopyTo(resp.KeyValues);

            pricingService.SendPriceResponse(resp);
        }

        /// <summary>
        /// Builds and send a PriceResponse message to YJ using the user assigned leg and market values.
        /// Open API key/values that have been added will also be sent
        /// </summary>
		public void SendPriceResponse()
		{
			try
			{
				session.Logger.Info("Sending price response", this);
				PriceResponse resp = new YJ.AppLink.Api.PriceResponse();
				resp.MarketDetailId = Id;
			
				if (theoreticalOverrideSpecified &&
					!Double.IsNaN(theoreticalOverride))
				{
					resp.TheoreticalOverrideSpecified = true;
					resp.TheoreticalOverride = theoreticalOverride;
				}

				if (volScalingNumberOverrideSpecified &&
					!Double.IsNaN(volScalingNumberOverride))
				{
					resp.VolatilityScalingNumberSpecified = true;
					resp.VolatilityScalingNumber = volScalingNumberOverride;
				}


				ArrayList values = new ArrayList();
                foreach (AbstractLeg leg in Options)
                {
                    values.Add(leg.GetLegValues());
                }

                foreach (AbstractLeg leg in Stocks)
                {
                    values.Add(leg.GetLegValues());
                }

                foreach (AbstractLeg leg in Swaps)
                {
                    values.Add(leg.GetLegValues());
                }
			
				resp.LegValues = new LegValues[values.Count];
				values.CopyTo(resp.LegValues);

                if (responseKeyValues != null)
                {
                    resp.KeyValues = new KeyValue[responseKeyValues.Count];
                    responseKeyValues.CopyTo(resp.KeyValues);
                }
			
				pricingService.SendPriceResponse(resp);
				lastPricedTime = DateTime.Now;
			}
			catch (Exception e)
			{
				session.OnSessionError("Failure sending price response", this, e);
				throw e;
			}
		}

        /// <summary>
        /// Clears the user assigned theoretical override value
        /// </summary>
		public void ClearTheoreticalOverride()
		{
			theoreticalOverrideSpecified = false;
			theoreticalOverride = double.NaN;
		}

        /// <summary>
        /// Assigns a theoretical override value to the market.
        /// </summary>
        /// <remarks>
        /// The user can assign this to override the value YJ Energy calculates 
        /// from the aggregate value of the market's legs.
        /// </remarks>
		public void SetTheoreticalOverride(double theo)
		{
			theoreticalOverrideSpecified = true;
			theoreticalOverride = theo;
		}

		/// <summary>
		/// Clears the user assigned volatility scaling number override value
		/// </summary>
		public void ClearVolatilityScalingNumberOverride()
		{
			volScalingNumberOverrideSpecified = false;
			volScalingNumberOverride = double.NaN;
		}

		/// <summary>
		/// Assigns a volatility scaling number override value to the market.
		/// </summary>
		/// <remarks>
		/// The user can assign this to override the value YJ Energy calculates 
		/// from the volatility.
		/// </remarks>
		public void SetVolatilityScalingNumberOverride(double num)
		{
			volScalingNumberOverrideSpecified = true;
			volScalingNumberOverride = num;
		}

		#endregion

	}
}
