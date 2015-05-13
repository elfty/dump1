using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal class CalcedValues
	{
		private SecurityData _data;

        private DateTime? _date;

		internal CalcedValues(SecurityData data)
		{
			_data = data;
		}

		internal void CalculateAdjustedValues()
		{
			AdjustedLow = _data.BidLow * _data.AdjustmentFactor2 / FixedValues.LastAdjustmentFactor2;
			if (AdjustedLow < 0)
			{
				AdjustedLow = -AdjustedLow;
			}

			AdjustedHigh = _data.AskHigh * _data.AdjustmentFactor2 / FixedValues.LastAdjustmentFactor2;
			if (AdjustedHigh < 0)
			{
				AdjustedHigh = -AdjustedHigh;
			}

			AdjustedClose = _data.ClosePrice * _data.AdjustmentFactor2 / FixedValues.LastAdjustmentFactor2;
			if (AdjustedClose < 0)
			{
				AdjustedClose = -AdjustedClose;
			}

		}


		internal object[] RawData { get { return _data.RawData; } }

		internal DateTime Date
		{
			get
			{
                if (_date == null)
                {
                    _date = _data.Date;
                }
                return (DateTime)_date;

			}
        	set
			{
				_date = value;
			}
		}

		internal double Average1Year_am_chg { get; set; }

		internal double AdjustedLow { get; set; }
		internal double AdjustedHigh { get; set; }
		internal double AdjustedClose { get; set; }

		internal double am_chg { get; set; }	// done - verified
		internal double am_rtn_1m { get; set; }	// done 
		internal double am_rtn_3m { get; set; }	// done 
		internal double am_rtn_6m { get; set; }	// done 
		internal double am_rtn_1y { get; set; }	// done 

		internal double am_range { get; set; }	// done - verified
		internal double am_range_5D { get; set; }
		internal double am_range_10D { get; set; }
		internal double am_range_1m { get; set; }
		internal double am_range_3m { get; set; }
		internal double am_range_6m { get; set; }
		internal double am_range_1y { get; set; }
		//public double am_var { get; set; }
		internal double am_corr { get; set; }
		internal double am_beta { get; set; }
		internal double am_cov_sp { get; set; }
		internal double am_corr_ndx { get; set; }
		internal double am_corr_rty { get; set; }
		internal double am_mkt_cap { get; set; }
		internal double am_var_spx { get; set; }

		internal double AdjustmentFactor2
		{
			get
			{
				return _data.AdjustmentFactor2;
			}
		}

		internal double ClosePrice
		{
			get
			{
				return _data.ClosePrice;
			}
		}

		internal double SharesOutstanding
		{
			get
			{
				return _data.SharesOutstanding;
			}
		}
	}
}
