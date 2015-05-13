using System;

using YJ.AppLink.Api;
using YJ.AppLink;

namespace YJ.AppLink.Pricing
{
    /// <summary>
    /// Provides information about a stock or cross leg of a market, 
    /// as well as getters and setters for user provided pricing.
    /// Also provides methods for sending updates to YJ Energy.
    /// </summary>	
    public class Stock : AbstractLeg
	{
		private StockLeg stockLeg;

        internal Stock(StockLeg leg, MarketData mkt, Session session)
            : base(mkt, session, leg.Direction, leg.Underlying)
		{
            this.stockLeg = leg;
		}

		#region public properties

        /// <summary>
        /// Gets or sets the user assigned Theoretical Price for the stock
        /// </summary>
		public double MyPrice
		{
			get { return this.myPrice; }
			set { this.myPrice = value; }
		}

        /// <summary>
        /// Gets the price of the stock if it is a cross, return double.NaN if not available
        /// </summary>
		public double CrossLevel
		{
            get 
            { 
                if (stockLeg.PriceSpecified)
                    return stockLeg.Price;

                return double.NaN;
            }
		}

        /// <summary>
        /// Gets the ratio (or delta) of the cross if it is specified, returns double>.NaN if not
        /// </summary>
        public double Ratio
        {
            get 
            {
                if (stockLeg.RatioSpecified)
                    return stockLeg.Ratio;

                return double.NaN;
            }
        }

        /// <summary>
        /// Gets a string representation of the leg
        /// </summary>
		public override string Description
		{
			get 
			{ 
				if (description == null)
				{
                    string d = (Direction > 0) ? "+" : "-";
                    string r = (double.IsNaN(Ratio) == false) ? Ratio.ToString() : "";
                    
                   
                    description = string.Format("{0} {1} STOCK", string.Format("{0}{1}", d, r), StockSymbol); 
				}
				
				return description; 
			}
		}

        /// <summary>
        /// Gets optional/extensible information for a given property key name.  
        /// Returns "" if not available
        /// </summary>
		public override string GetLegProperty(string propertyName)
		{
			if (propertyName != null &&
				stockLeg.KeyValues != null &&
				stockLeg.KeyValues.Length > 0)
			{
				foreach (KeyValue keyValue in stockLeg.KeyValues)
				{
					if (keyValue.Key.ToLower().Equals(propertyName.ToLower()))
						return keyValue.Value;
				}
			}

			return "";
		}


		#endregion

		#region internal properties

		internal string LegId
		{
			get { return stockLeg.LegId; }	
		}

        internal string CrossId
        {
            get { return stockLeg.CrossId; }
        }
		#endregion

		#region internal methods

		internal override LegValues GetLegValues()
		{
			try
			{
				LegValues values = new LegValues();
                values.LegId = LegId;
		
				values.TheoreticalPriceSpecified = true;
				values.TheoreticalPrice = myPrice;

				return values;
			}
			catch (Exception e)
			{
				session.OnSessionError("Failure getting leg values", this, e);
				throw e;
			}
		}

		#endregion

	}
}
