using System;

using YJ.AppLink.Api;
using YJ.AppLink;

namespace YJ.AppLink.Pricing
{
	/// <summary>
	/// An abstract class that is a base for Option and Stock legs.
    /// Provides getters and setters for leg data, analytics values and
    /// a method for sending leg price updates.
	/// </summary>
	public abstract class AbstractLeg
	{		
		protected MarketData mkt;
		protected Session session;
        protected int direction;
        protected Underlying underlying;

		protected double theoretical = double.NaN;
		protected double delta = double.NaN;
		protected double gamma = double.NaN;
		protected double vega = double.NaN;
		protected double theta = double.NaN;
		protected double rho = double.NaN;
		protected double impliedVol = double.NaN;
		protected double myPrice = double.NaN;
        protected double vol = double.NaN;

		protected string description;

        protected AbstractLeg(MarketData mkt, Session session, int direction, Underlying underlying)
		{
			this.mkt = mkt;
			this.session = session;
            this.direction = direction;
            this.underlying = underlying;
		}

		private AbstractLeg () { }

		#region public properties

		/// <summary>
		/// Gets a reference to the parent MarketData
		/// </summary>
        public MarketData MarketData
		{
			get { return this.mkt; }
		}

        /// <summary>
        /// 1 = Long Leg, -1 = Short Leg
        /// </summary>
        public int Direction
        {
            get { return this.direction; }
        }

        public string StockSymbol
        {
            get { return this.underlying.Symbol; }
        }

        public string StockName
        {
            get { return this.underlying.Name; }
        }


		#endregion

		#region public methods

        /// <summary>
        /// Sends a price update for the parent MarketData
        /// </summary>
		public void SendLegUpdate()
		{
			try
			{
				session.Logger.Info("Sending leg update", this);
				mkt.SendPriceResponse();
			}
			catch (Exception e)
			{
				session.OnSessionError("Failure sending leg update", this, e);
				throw e; 
			}
		}

		public override string ToString()
		{
			return Description;
		}

		#endregion

		#region abstract methods
		
		/// <summary>
		/// Gets a string description of a leg
		/// </summary>
        public abstract string Description { get; }


        /// <summary>
        /// Provides a method to get special, optional properties of a leg
        /// </summary>
        /// <param name="propertyName">The name of to be defined properties</param>
        /// <returns>Returns a string representation of the value, or "" if not available.</returns>
		public abstract string GetLegProperty(string propertyName);
		
		internal abstract LegValues GetLegValues();

		#endregion

	}
}
