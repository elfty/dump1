using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal class IndexData
	{
		public IndexRawData SnP;
		public IndexRawData Nasdaq;
		public IndexRawData Russell;
		
		public double am_chg_ndx;
		public double am_chg_sp;
		public double am_chg_rty;

		public double? spx
		{
			get
			{
				return SnP.Value;
			}
		}
		public double? ndx
		{
			get
			{
				return Nasdaq.Value;
			}
		}
		public double? rty
		{
			get
			{
				return Russell.Value;
			}
		}
	}
}
