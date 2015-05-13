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
    public class Swap : AbstractLeg
	{
		private SwapLeg leg;

        internal Swap(SwapLeg leg, MarketData mkt, Session session)
            : base(mkt, session, leg.Direction, leg.Underlying)
		{
            this.leg = leg;
		}

		#region public properties

        /// <summary>
        /// Gets or sets the user assigned theoretical price of the option
        /// </summary>
        public double TheoreticalPrice
        {
            get { return this.theoretical; }
            set { this.theoretical = value; }
        }

        public string SwapType
        {
            get { return leg.SwapType; }
        }

        public string Term
        {
            get { return leg.Term; }
        }

        public double ReferencePrice
        {
            get
            {
                if (leg.ReferencePriceSpecified)
                    return leg.ReferencePrice;

                return double.NaN;
            }
        }

        public double ReferenceDelta
        {
            get
            {
                if (leg.ReferenceDeltaSpecified)
                    return leg.ReferenceDelta;

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
                return leg.Ratio;
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
                    
                   
                    description = string.Format("{0}{1} {2}", d, r, leg.Description); 
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
				leg.KeyValues != null &&
				leg.KeyValues.Length > 0)
			{
				foreach (KeyValue keyValue in leg.KeyValues)
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
			get { return leg.LegId; }	
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
                values.TheoreticalPrice = TheoreticalPrice;

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
