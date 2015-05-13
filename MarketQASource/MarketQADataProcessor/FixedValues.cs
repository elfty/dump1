using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal static class FixedValues
	{
		public static int RowsToProcess = 3730;
        public static int DaysIn1Week = 5;
        public static int DaysIn2Weeks = 10;
		public static int DaysInMonth = 21;
		public static int DaysIn3Months = 21 * 3;
		public static int DaysIn6Months = 21 * 6;
		public static int DaysIn12Months = 21 * 12;
		public static int DaysIn24Months = 21 * 24;
		public static int DaysIn36Months = 21 * 36;

		public static double LastAdjustmentFactor2;
	}
}
