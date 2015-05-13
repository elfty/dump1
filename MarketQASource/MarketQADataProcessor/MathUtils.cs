using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal static class MathUtils
	{
		internal static double StdDev(double variance)
		{
			return Math.Sqrt(variance);
		}

		internal static double StDev(double[] data, double average, int startFrom, int population)
		{
			// VAR = sum of ((i - avg)^2) / len
			return StdDev(Variance(data, average, startFrom, population));
		}

		internal static double Variance(double[] data, double average, int startFrom, int population)
		{
			double sum = 0;

			for (int i = startFrom; i < startFrom + population; i++)
			{
				sum += Math.Pow((data[i] - average), 2);
			}

			return sum / population;
		}

		internal static double Variance(double[] data)
		{
			int len = data.Length;
			// Get average
			double avg = Average(data);

			double sum = 0;
			for (int i = 0; i < data.Length; i++)
				sum += Math.Pow((data[i] - avg), 2);
			return sum/len;
		}


		internal static double Average(double[] data, int startFrom, int population)
		{
			double sum = 0;
			int end = startFrom + population;
			for (int i = startFrom; i < end; i++)
			{
				sum += data[i];
			}

			return sum / population;	
		}

		internal static double Average(double[] data)
		{
			//int len = data.Length;
			//if (len == 0)
			//	throw new Exception("No data");

			double sum = 0;
			for (int i = 0; i < data.Length; i++)
			{
				sum += data[i];
			}

			return sum / data.Length;
		}
	}
}
