using System;

using YJ.AppLink.Api;
using YJ.AppLink;

namespace YJ.AppLink.Pricing
{

	/// <summary>
	/// Provides information about an option leg of a market, 
    /// as well as getters and setters for user provided pricing and greek values.
    /// Also provides methods for sending updates to YJ Energy.
	/// </summary>
    public class Option : AbstractLeg
	{
		private Stock cross;
		private OptionLeg option;

		internal Option (OptionLeg option, MarketData mkt, Session session)
            : base(mkt, session, option.Direction, option.Underlying)
		{
			this.option = option;
		}

		#region public properties

        /// <summary>
        /// Gets the term of the underlying, i.e "F10"
        /// </summary>
        public string Term
        {
            get
            {
                return option.Term;
            }
        }

        /// <summary>
        /// Number of the legs in the strategy
        /// </summary>
        public double Ratio
        {
            get
            {
                return option.Ratio;
            }
        }

        /// <summary>
        /// Gets the OSCI code, returns "" if not available
        /// </summary>
        public string OSICode
        {
            get
            {
                return option.OSICode ?? string.Empty;
            }
        }
        
        /// <summary>
        /// Gets the option type value, either CALL or PUT
        /// </summary>
        public string OptionType
        {
            get
            {
                return option.OptionType;
            }
        }

        /// <summary>
        /// Get the exercise type of the option, either American, European or Asian.
        /// Returns "" if not available
        /// </summary>
        public string ExerciseType
        {
            get 
            { 
                if (option.ExerciseType == null)
                    return "";

                return option.ExerciseType; 
            }
        }

        /// <summary>
        /// Gets the style of the option, such as Monthly, SDO, CSO, APO, etc.
        /// Returns "" if not available
        /// </summary>
		public string OptionStyle
		{
            get
            {
                if (option.Style == null)
                    return "";
                
                return option.Style; 
            }
		}

        /// <summary>
        /// Gets the option's strike value
        /// </summary>
		public double Strike
		{
			get { return option.Strike; }
		}

        /// <summary>
        /// Gets the option's strike type, either "Price" or "Percent"
        /// </summary>
        public string StrikeType
        {
            get { return option.StrikeType; }
        }


        //may expose later for crack spreads
        //public Underlying Underlying2
        //{
        //    get { return this.underlying2; }
        //}

        /// <summary>
        /// Gets the stock leg if the option is crossed
        /// </summary>
		public Stock Cross
		{
			get { return this.cross; }
		}

        /// <summary>
        /// Gets or sets the user assigned theoretical price of the option
        /// </summary>
		public double TheoreticalPrice
		{
			get { return this.theoretical; }
			set { this.theoretical = value; }
		}

        /// <summary>
        /// Gets or sets the user assigned delta of the option
        /// </summary>
		public double Delta
		{
			get { return this.delta; }
			set { this.delta = value; }
		}

        /// <summary>
        /// Gets or sets the user assigned gamma of the option
        /// </summary>
		public double Gamma
		{
			get { return this.gamma; }
			set { this.gamma = value; }
		}

        /// <summary>
        /// Gets or sets the user assigned vega of the option
        /// </summary>
		public double Vega
		{
			get { return this.vega; }
			set { this.vega = value; }
		}

        /// <summary>
        /// Gets or sets the user assigned theta of the option
        /// </summary>
		public double Theta
		{
			get { return this.theta; }
			set { this.theta = value; }
		}

        /// <summary>
        /// Gets or sets the user assigned rho of the option
        /// </summary>
		public double Rho
		{
			get { return this.theta; }
			set { this.theta = value; }
		}

        /// <summary>
        /// Gets or sets the user assigned implied volatility of the option
        /// </summary>
        public double ImpliedVol
        {
            get { return this.impliedVol; }
            set { this.impliedVol = value; }
        }

        /// <summary>
        /// Gets or sets the user assigned volatility that was used to calculate the option's theoretical price
        /// </summary>
        public double Vol
        {
            get { return this.vol; }
            set { this.vol = value; }
        }

        /// <summary>
        /// Gets a description of the option
        /// </summary>
		public override string Description
		{
			get 
			{ 
				if (description == null)
				{
                    string d = (Direction > 0) ? "+" : "-";
                    string rString = d + Ratio.ToString();



                    if (string.IsNullOrEmpty(OSICode) == false)
                    {
                        description = string.Format("{0} {1} {2} {3} {4} ({5})",
                            rString, StockSymbol, Term, Strike.ToString(), OptionType, OSICode);
                    }
                    else
                    {
                        description = string.Format("{0} {1} {2} {3} {4}",
                            rString, StockSymbol, Term, Strike.ToString(), OptionType);
                    }

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
				option.KeyValues != null &&
				option.KeyValues.Length > 0)
			{
				foreach (KeyValue keyValue in option.KeyValues)
				{
					if (keyValue.Key.ToLower().Equals(propertyName.ToLower()))
						return keyValue.Value;
				}
			}

			return "";
		}

		#endregion

		#region internal properties

		internal string CrossId
		{
			get { return option.CrossId; }	
		}

        internal string LegId
        {
            get { return option.LegId; }
        }

		#endregion

		#region internal methods

		internal void SetCross(Stock cross)
		{
			this.cross = cross;
		}
		
		internal override LegValues GetLegValues()
		{
			try
			{
				LegValues values = new LegValues();
                values.LegId = LegId;

				values.TheoreticalPriceSpecified = true;
				values.TheoreticalPrice = theoretical;

				values.DeltaSpecified = true;
				values.Delta= delta;

				values.GammaSpecified = true;
				values.Gamma = gamma;

				values.VegaSpecified = true;
				values.Vega = vega;
				
				values.ThetaSpecified = true;
				values.Theta = theta;

				values.RhoSpecified = true;
				values.Rho = rho;

                values.VolSpecified = true;
                values.Vol = vol;

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
