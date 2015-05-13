using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal class ResultsArrays
	{
        internal double[] am_vol_5;
        internal double[] am_vol_10;
		internal double[] am_vol_21;
		internal double[] am_vol_63;
		internal double[] am_vol_126;
		internal double[] am_vol_252;
		internal double[] am_vol_504;
		internal double[] am_vol_756;

		internal double[] am_chg;
		//internal double[] am_var;

		internal double[] Pearson_am_corr;
		internal double[] Pearson_am_corr_ndx;
		internal double[] Pearson_am_corr_rty;
		
		internal double[] am_cov_sp;
		internal double[] am_beta;
		internal double[] am_var_spx;

		internal DateTime[] Dates;
		
		internal double[] Variance12Months_am_chg;
		internal double[] Variance12Months_Nasdaq;
		internal double[] Variance12Months_Russell;
		internal double[] Variance12Months_SnP;
		
		internal double[] Average12Months_am_chg_ndx;
		internal double[] Average12Months_am_chg_rty; 
		internal double[] Average12Months_am_chg_sp;


        internal double[] Average5_am_chg;
        internal double[] Average10_am_chg;
		internal double[] Average21_am_chg;
		internal double[] Average63_am_chg;
		internal double[] Average126_am_chg;
		internal double[] Average252_am_chg;
		internal double[] Average504_am_chg;
		internal double[] Average756_am_chg;

        internal double[] StdDev5_am_chg;
        internal double[] StdDev10_am_chg;
        internal double[] StdDev21_am_chg;
		internal double[] StdDev63_am_chg;
		internal double[] StdDev126_am_chg;
		internal double[] StdDev252_am_chg;
		internal double[] StdDev504_am_chg;
		internal double[] StdDev756_am_chg;

		public ResultsArrays(int capacity)
		{
			Capacity = capacity;

			// Should use reflection to iterate through the fields
			am_chg = new double[Capacity];
			//am_var = new double[Capacity];

			Pearson_am_corr = new double[Capacity];
			Pearson_am_corr_ndx = new double[Capacity];
			Pearson_am_corr_rty = new double[Capacity];

			am_cov_sp = new double[Capacity];
			am_beta = new double[Capacity];
			am_var_spx = new double[Capacity];

			Dates = new DateTime[Capacity];

			Variance12Months_am_chg = new double[Capacity];
			Variance12Months_Nasdaq = new double[Capacity];
			Variance12Months_Russell = new double[Capacity];
			Variance12Months_SnP = new double[Capacity];

			Average12Months_am_chg_ndx = new double[Capacity];
			Average12Months_am_chg_rty = new double[Capacity];
			Average12Months_am_chg_sp = new double[Capacity];

            Average5_am_chg = new double[Capacity];
            Average10_am_chg = new double[Capacity];
			Average21_am_chg = new double[Capacity];
			Average63_am_chg = new double[Capacity];
			Average126_am_chg = new double[Capacity];
			Average252_am_chg = new double[Capacity];
			Average504_am_chg = new double[Capacity];
			Average756_am_chg = new double[Capacity];

            StdDev5_am_chg = new double[Capacity];
            StdDev10_am_chg = new double[Capacity];
			StdDev21_am_chg = new double[Capacity];
			StdDev63_am_chg = new double[Capacity];
			StdDev126_am_chg = new double[Capacity];
			StdDev252_am_chg = new double[Capacity];
			StdDev504_am_chg = new double[Capacity];
			StdDev756_am_chg = new double[Capacity];

            am_vol_5 = new double[Capacity];
            am_vol_10 = new double[Capacity];
			am_vol_21 = new double[Capacity];
			am_vol_63 = new double[Capacity];
			am_vol_126 = new double[Capacity];
			am_vol_252 = new double[Capacity];
			am_vol_504 = new double[Capacity];
			am_vol_756 = new double[Capacity];
		}

		public int Capacity { get; private set; }
	}

	internal class SourceData
	{
		//internal double[] am_chg;
		
		internal double[] am_chg_ndx;
		internal double[] am_chg_rut; 
		internal double[] am_chg_spx;
		
		public SourceData(int capacity)
		{
			Capacity = capacity;

			//am_chg = new double[Capacity];

			am_chg_ndx = new double[Capacity];
			am_chg_rut = new double[Capacity];
			am_chg_spx = new double[Capacity];
		}

		public int Capacity { get; private set; }
	}
}
