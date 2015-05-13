using System;
using System.Collections.Generic;

namespace MarketQADataProcessor
{
	internal class SecurityCalcedValues
	{
		List<CalcedValues> values;
		//List<IndexData> indexData;
		ResultsArrays results;
		SourceData sourceData;
        DBHelper _dbHelper = new DBHelper();

		public SecurityCalcedValues(int securityID, int numOfRows)
		{
			SecurityID = securityID;
			values = new List<CalcedValues>(numOfRows);

			for (int i = 0; i < numOfRows; i++)
			{
				values.Add(null);
			}
		}

		public int SecurityID { get; private set; }
		public int SecurityNonIvyID { get; set; }

		public List<CalcedValues> Values
		{
			get
			{
				return values;
			}
		}

		internal SourceData Source
		{
			get
			{
				return sourceData;
			}
		}

		internal ResultsArrays Results
		{
			get
			{
				return results;
			}
		}

		internal void Add(int position, CalcedValues data)
		{
			// TODO Add indexer
			values[position] = data;
		}

		internal void SetIndexData(List<IndexData> indexData)
		{
			// Quick workaround. Later convert the values
			sourceData = new SourceData(indexData.Count);

			// TODO Remove this temp code - get rid of indexData
			double[] tmpNdx = sourceData.am_chg_ndx;
			double[] tmpRut = sourceData.am_chg_rut;
			double[] tmpSpx = sourceData.am_chg_spx;


			for (int i = 0; i < indexData.Count; i++)
			{
				tmpNdx[i] = indexData[i].ndx ?? double.NaN;
				tmpRut[i] = indexData[i].rty ?? double.NaN;
				tmpSpx[i] = indexData[i].spx ?? double.NaN;
			}
		}

		internal void ClearResults()
		{
			results = null;	// Let the GC handle the cleanup
		}

        public string Calculate(List<IndexData> indexData)
		{
			// TODO Rewrite the calculations: organize and restructure
			results = new ResultsArrays(sourceData.am_chg_spx.Length);


			CalcedValues tmp = values[values.Count - 1];
			if (tmp == null)
			{
				return "Missing last entry. Cannot get AdjustmentFactor2";
			}

			FixedValues.LastAdjustmentFactor2 = tmp.AdjustmentFactor2;

			// Prep the data
			
			for (int i = 0; i < values.Count; i++)
			{
				tmp = values[i];
				if (tmp != null)
				{
					tmp.CalculateAdjustedValues();	
				}
			}

			int firstEntry = 0;
			double rangeSum = 0;
			double yearSum = 0;
			double changeSum = 0;

            // GET ADR SHARE to ORD RATIO

            double am_adr_ratio = 0;
            string importsDB = "OPTIONS";
            string importsSchema = "dbo";
            string securitiesTable = "INSTRUMENT_REF";
            string errorMsg1 = null;

            am_adr_ratio = _dbHelper.GetADRRatio(importsDB, importsSchema, securitiesTable, SecurityNonIvyID, ref errorMsg1);

            if (am_adr_ratio == 0)
            {
                am_adr_ratio = 1;
            }

			#region range and chg
			List<int> missingEntries = new List<int>();
			int numOfValues = 0;
			for (int i = firstEntry; i < values.Count; i++)
			{
				numOfValues++;

				CalcedValues calced = values[i];
				if (calced == null)
				{

					missingEntries.Add(i);

					values[i] = (i == firstEntry) ? new CalcedValues(new SecurityData()) : new CalcedValues(new SecurityData(values[i-1].RawData));
                    values[i].Date = indexData[i].SnP.Date;

					calced = values[i];
				}

				calced.am_mkt_cap = calced.ClosePrice * (calced.SharesOutstanding / am_adr_ratio) / 1000;

				// TODO Clean up. Make it less efficient, but easier to read and maintain
				// TODO Remove the i > 0 check
				if (i > 0)
				{
					CalcedValues prevDay = values[i - 1];

					double prevDayAdjustedClose;
					double amRange5D;
					double amRange1m;

					if (prevDay == null)
					{
						prevDayAdjustedClose = double.NaN;
						amRange5D = double.NaN;
						amRange1m = double.NaN;
					}
					else
					{
						prevDayAdjustedClose = prevDay.AdjustedClose;
						amRange5D = prevDay.am_range_5D;
						amRange1m = prevDay.am_range_1m;
					}

					if (calced.AdjustedClose == 0)
					{
						calced.AdjustedClose = prevDayAdjustedClose;
						calced.AdjustedHigh = prevDayAdjustedClose;
						calced.AdjustedLow = prevDayAdjustedClose;
					}

                    //if (calced.Date == Convert.ToDateTime("10/27/2011"))
                       // Console.WriteLine("hi");
					calced.am_chg = Math.Log(calced.AdjustedClose / prevDayAdjustedClose);
					if (double.IsNaN(calced.am_chg) || double.IsInfinity(calced.am_chg))
					{
						calced.am_chg = 0;
					}

					changeSum += calced.am_chg;


					calced.am_range = (calced.AdjustedHigh - calced.AdjustedLow) / prevDayAdjustedClose;
					if (double.IsNaN(calced.am_range))
					{
						// HACK
						calced.am_range = 0;
					}
					else if (double.IsInfinity(calced.am_range))
					{
						// HACK
						LoggingHelper.LogError("Previous day's AdjustedClose is 0. Ivy Security ID {1}; Security ID: {0}; date: {2}; index: {3}", SecurityNonIvyID, SecurityID, values[i].Date, i);
						calced.am_range = 0;
					}
					rangeSum += calced.am_range;

					if (i == firstEntry + 4)
					{
						// 1..5
						calced.am_range_5D = rangeSum / 5;
						if (double.IsNaN(calced.am_range_5D))
						{
							// HACK
							LoggingHelper.LogError("am_range_5D is NaN. Ivy Security ID {0}; Security ID: {1}; date: {3}; index: {4}", SecurityNonIvyID, SecurityNonIvyID, values[i].Date, i);
							calced.am_range_5D = 0;
						}
					}
					else if (i > firstEntry + 4)
					{
						calced.am_range_5D = amRange5D - values[i - 5].am_range / 5 + calced.am_range / 5;
						if (double.IsNaN(calced.am_range_5D))
						{
							// HACK
							LoggingHelper.LogError("am_range_5D is NaN. Ivy Security ID {0}; Security ID: {1}; date: {3}; index: {4}", SecurityNonIvyID, SecurityNonIvyID, values[i].Date, i);
							calced.am_range_5D = 0;
						}


						if (i > firstEntry + 8)
						{
							calced.am_range_10D = (values[i - 5].am_range_5D + calced.am_range_5D) / 2;

							// 21 days
							if (i == FixedValues.DaysInMonth)
							{
								calced.am_range_1m = rangeSum / FixedValues.DaysInMonth;
							}
							else if (i > FixedValues.DaysInMonth)
							{
								calced.am_range_1m =
									amRange1m
									- values[i - FixedValues.DaysInMonth].am_range / FixedValues.DaysInMonth
									+ calced.am_range / FixedValues.DaysInMonth;

								if (i > FixedValues.DaysIn3Months - 1)
								{
									calced.am_range_3m =
										(values[i - FixedValues.DaysInMonth * 2].am_range_1m
										 + values[i - FixedValues.DaysInMonth].am_range_1m
										 + calced.am_range_1m
										) / 3;
								}

								if (i > FixedValues.DaysIn6Months - 1)
								{
									calced.am_range_6m =
										(
											values[i - FixedValues.DaysIn3Months].am_range_3m
											+ calced.am_range_3m
										) / 2;
								}

								if (i > FixedValues.DaysIn12Months - 1)
								{
									calced.am_range_1y =
										(
											values[i - FixedValues.DaysIn6Months].am_range_6m
											+ calced.am_range_6m
										) / 2;
								}


								if (i == FixedValues.DaysIn12Months - 1)
								{
									calced.Average1Year_am_chg = changeSum / FixedValues.DaysIn12Months;
									yearSum = changeSum;
								}
								else if (i > FixedValues.DaysIn12Months - 1)
								{
									yearSum = yearSum - values[i - FixedValues.DaysIn12Months].am_chg + values[i].am_chg;
									calced.Average1Year_am_chg = yearSum / FixedValues.DaysIn12Months;
								}
							}
						}
					}
				}
			}

			#endregion

			if (missingEntries.Count > 0)
			{
				LoggingHelper.LogWarn("Missing entry for Security ID {0}; row: {1}", SecurityNonIvyID, string.Join(",", missingEntries));
			}

			#region New code


			double[] tmpValues = results.am_chg;
			for (int i = 0; i < values.Count; i++)
			{
				tmpValues[i] = values[i].am_chg;
			}


			

			/*int firstValidIndexesRow = 0;
			for (int i = 0; i < sourceData.am_chg_ndx.Length; i++)
			{
				if (double.IsNaN(sourceData.am_chg_ndx[i]))
				{
					firstValidIndexesRow = i;
				}
				else
				{
					break;
				}
			}*/

			#endregion

			int startFrom = 0;

			// Calc am_var
/*
			int population = FixedValues.DaysIn12Months;
			for (int i = population; i < values.Count; i++)
			{
				startFrom = i - (population - 1);
				results.am_var[i] = MathUtils.Variance(sourceData.am_chg, values[i].Average1Year, startFrom, FixedValues.DaysIn12Months);
			}
*/
            int population = FixedValues.DaysIn12Months;
            if (values.Count > FixedValues.DaysIn12Months)
            {
                //population = values.Count;
                CalculateFirstAndRollingAverages(sourceData.am_chg_ndx, results.Average12Months_am_chg_ndx, startFrom, population);
                CalculateFirstAndRollingAverages(sourceData.am_chg_rut, results.Average12Months_am_chg_rty, startFrom, population);
                CalculateFirstAndRollingAverages(sourceData.am_chg_spx, results.Average12Months_am_chg_sp, startFrom, population);
            }

			// TODO Clean up
			// There is no entry in am_range for position 0 in the DB, but it is present in the dataset - starting from x+1
			for (int i = population - 1; i < values.Count; i++)
			{
				CalcedValues data = values[i];

				startFrom = i - (population - 1);
				results.Variance12Months_am_chg[i] = MathUtils.Variance(results.am_chg, values[i].Average1Year_am_chg, startFrom, population);
				//results.am_var[i] = MathUtils.Variance(results.am_chg, values[i].Average1Year_am_chg, startFrom, days);

				Stats ndx;
				Stats rty;
				Stats sp;

				//GetCorrelations(results.am_var[lastPosition], population, values, sourceData, startFrom, out ndx, out rty, out sp);
				GetCorrelations(results.Variance12Months_am_chg[i], population, values, sourceData, startFrom, out ndx, out rty, out sp);

				results.Pearson_am_corr[i] = sp.Pearson;
				results.Pearson_am_corr_ndx[i] = ndx.Pearson;
				results.Pearson_am_corr_rty[i] = rty.Pearson;

				results.am_cov_sp[i] = sp.Covariance;
				results.am_beta[i] = sp.Covariance / sp.Variance;
				results.am_var_spx[i] = sp.Variance;
				
				data.am_corr = sp.Pearson;
				data.am_corr_ndx = ndx.Pearson;
				data.am_corr_rty = rty.Pearson;

				data.am_cov_sp = sp.Covariance;
				data.am_beta = data.am_cov_sp / sp.Variance;
				data.am_var_spx = sp.Variance;
			}


			double first = 0;
			decimal month1Return = 0;
			double last = 0;



			// Slower, but easier to read & maintain
			int firstValidRow = 1;

            CalculateRangeOfVolatilities(results.am_chg, results.Average5_am_chg, results.StdDev5_am_chg, results.am_vol_5, firstValidRow, FixedValues.DaysIn1Week);
            CalculateRangeOfVolatilities(results.am_chg, results.Average10_am_chg, results.StdDev10_am_chg, results.am_vol_10, firstValidRow, FixedValues.DaysIn2Weeks);
            CalculateRangeOfVolatilities(results.am_chg, results.Average21_am_chg, results.StdDev21_am_chg, results.am_vol_21, firstValidRow, FixedValues.DaysInMonth);
            CalculateRangeOfVolatilities(results.am_chg, results.Average63_am_chg, results.StdDev63_am_chg, results.am_vol_63, firstValidRow, FixedValues.DaysIn3Months);
			CalculateRangeOfVolatilities(results.am_chg, results.Average126_am_chg, results.StdDev126_am_chg, results.am_vol_126, firstValidRow, FixedValues.DaysIn6Months);
			CalculateRangeOfVolatilities(results.am_chg, results.Average252_am_chg, results.StdDev252_am_chg, results.am_vol_252, firstValidRow, FixedValues.DaysIn12Months);
			CalculateRangeOfVolatilities(results.am_chg, results.Average504_am_chg, results.StdDev504_am_chg, results.am_vol_504, firstValidRow, FixedValues.DaysIn24Months);
			CalculateRangeOfVolatilities(results.am_chg, results.Average756_am_chg, results.StdDev756_am_chg, results.am_vol_756, firstValidRow, FixedValues.DaysIn36Months);



			// Calc 1 month return)
			for (int i = 0; i < values.Count; i++)
			{
				month1Return += (decimal)values[i].am_chg;
				if (i > FixedValues.DaysInMonth)
				{
					month1Return -= (decimal)values[i - FixedValues.DaysInMonth].am_chg;
				}

				if (i > FixedValues.DaysInMonth - 1)
				{
					values[i].am_rtn_1m = (double)month1Return;
				}
			}

			// Calc 3 month return
			for (int i = FixedValues.DaysInMonth * 3; i < values.Count; i++)
			{
				values[i].am_rtn_3m =
					values[i].am_rtn_1m
					+ values[i - FixedValues.DaysInMonth].am_rtn_1m
					+ values[i - FixedValues.DaysInMonth * 2].am_rtn_1m;
			}

			// Calc 6 month return
			for (int i = FixedValues.DaysInMonth * 6; i < values.Count; i++)
			{
				values[i].am_rtn_6m =
					values[i].am_rtn_3m
					+ values[i - FixedValues.DaysInMonth * 3].am_rtn_3m;
			}

			// Calc 12 month return
			for (int i = FixedValues.DaysInMonth * 12; i < values.Count; i++)
			{
				values[i].am_rtn_1y =
					values[i].am_rtn_6m
					+ values[i - FixedValues.DaysInMonth * 6].am_rtn_6m;
			}

			return null;
		}


		private static void CalculateRangeOfVolatilities(double[] sourceData, double[] averages, double[] stDevs, double[] volatilities, int firstValidRow, int population)
		{
            if (sourceData.Length > population)
            {
                CalculateFirstAndRollingAverages(sourceData, averages, firstValidRow, population);
                CalculateRangeOfStdDevs(sourceData, averages, stDevs, population, firstValidRow);
                //CalculateRangeOfVolatilities(stDevs, volatilities, 252);

                double multiplier = Math.Sqrt(252);
                for (int i = firstValidRow + population - 1; i < volatilities.Length; i++)
                {
                    volatilities[i] = stDevs[i] * multiplier;
                }
            }
		}

		private static void CalculateRangeOfStdDevs(double[] sourceData, double[] averages, double[] results, int population, int startFrom)
		{
			int position;
			for (int i = startFrom; i <= averages.Length - population; i++)
			{
				// position of the last row for the population: i + population - 1
				position = i + population - 1;
				results[position] = MathUtils.StDev(sourceData, averages[position], i, population);
			}
		}

		private static void CalculateFirstAndRollingAverages(double[] source, double[] dest, int startFrom, int population)
		{
			int position = startFrom + population - 1;
			dest[position] = MathUtils.Average(source, startFrom, population);
			while (double.IsNaN(dest[position]))
			{
				startFrom++;
				if (startFrom + population == source.Length)
				{
					return;
				}

				dest[position] = MathUtils.Average(source, startFrom, population);
			}

			CalculateRollingAverages(source, dest, startFrom, population);
		}

		private static void CalculateRollingAverages(double[] sourceData, double[] calculatedAverages, int startFrom, int population)
		{
			for (int i = startFrom + population; i < calculatedAverages.Length; i++)
			{
				NextAverage(sourceData, calculatedAverages, i, population);
			}
		}

		private static void NextAverage(double[] source, double[] dest, int position, int population)
		{
			dest[position] = dest[position - 1]
			                 - source[position - population] / population
			                 + source[position] / population;
		}

		// STDEV = Sqrt(VAR)
		// VAR = STDEV ^2
		// VAR = sum of ((i - avg)^2) / len

		private void GetCorrelations(double variance, int population, List<CalcedValues> values, SourceData sourceData, int startFrom, out Stats ndx, out Stats rty, out Stats sp)
		{
			int lastPosition = startFrom + population - 1;
			
			double avgXNew = results.Average252_am_chg[lastPosition];
			double avgX = values[lastPosition].Average1Year_am_chg;

			double stdevX = MathUtils.StdDev(variance);

			string file = "Data.csv";
			//FileHelper.CreateFile(@"C:\", file);

			ndx.Covariance = 0;
			rty.Covariance = 0;
			sp.Covariance = 0;
			double covXY = 0;
			for (int i = startFrom; i <= lastPosition; i++)
			{
				DateTime secDate = values[i].Date;
				DateTime idxDate = results.Dates[i];

				double am_chg_ndx = sourceData.am_chg_ndx[i];
				double am_chg_rty = sourceData.am_chg_rut[i];
				double am_chg_sp = sourceData.am_chg_spx[i];

				// for the range: tmp += (x[i] - avgX) * (y[i] - avgY)
				// covariance = tmp / range

				//double productX = data.am_chg - avgX;
				double productX = results.am_chg[i] - avgX;
				//FileHelper.WriteLine(file, secDate + "," + results.Dates[i] + "," + results.am_chg[i] + "," + avgX + "," + am_chg_sp + "," + results.Average12Months_am_chg_sp[lastPosition] + ",");

				ndx.Covariance += productX * (am_chg_ndx - results.Average12Months_am_chg_ndx[lastPosition]);
				rty.Covariance += productX * (am_chg_rty - results.Average12Months_am_chg_rty[lastPosition]);
				sp.Covariance += productX * (am_chg_sp - results.Average12Months_am_chg_sp[lastPosition]);

				covXY += productX * (am_chg_sp - results.Average12Months_am_chg_sp[i]);
			}
			//FileHelper.Close(file);
			ndx.Covariance /= population;
			rty.Covariance /= population;
			sp.Covariance /= population;


			ndx.Variance = MathUtils.Variance(sourceData.am_chg_ndx, results.Average12Months_am_chg_ndx[lastPosition], startFrom, population);
			rty.Variance = MathUtils.Variance(sourceData.am_chg_rut, results.Average12Months_am_chg_rty[lastPosition], startFrom, population);
			sp.Variance = MathUtils.Variance(sourceData.am_chg_spx, results.Average12Months_am_chg_sp[lastPosition], startFrom, population);

			ndx.Pearson = ndx.Covariance / (stdevX * MathUtils.StdDev(ndx.Variance));
			rty.Pearson = rty.Covariance / (stdevX * MathUtils.StdDev(rty.Variance));
			sp.Pearson = sp.Covariance / (stdevX * MathUtils.StdDev(sp.Variance));
		}

		public static double pearsoncorr2(double[] x, double[] y, int from, int n)
		{
			double result = 0;
			int i = 0;
			double xmean = 0;
			double ymean = 0;
			double v = 0;
			double x0 = 0;
			double y0 = 0;
			double s = 0;
			bool samex = new bool();
			bool samey = new bool();
			double xv = 0;
			double yv = 0;
			double t1 = 0;
			double t2 = 0;

			//
			// Special case
			//
			if (n <= 1)
			{
				result = 0;
				return result;
			}

			//
			// Calculate mean.
			//
			//
			// Additonally we calculate SameX and SameY -
			// flag variables which are set to True when
			// all X[] (or Y[]) contain exactly same value.
			//
			// If at least one of them is True, we return zero
			// (othwerwise we risk to get nonzero correlation
			// because of roundoff).
			//
			xmean = 0;
			ymean = 0;
			samex = true;
			samey = true;
			x0 = x[0];
			y0 = y[0];
			v = (double)1 / (double)n;
			for (i = from; i <= n - 1; i++)
			{
				s = x[i];
				samex = samex & (double)(s) == (double)(x0);
				xmean = xmean + s * v;
				s = y[i];
				samey = samey & (double)(s) == (double)(y0);
				ymean = ymean + s * v;
			}
			if (samex | samey)
			{
				result = 0;
				return result;
			}

			//
			// numerator and denominator
			//
			s = 0;
			xv = 0;
			yv = 0;
			for (i = from; i <= n - 1; i++)
			{
				t1 = x[i] - xmean;
				t2 = y[i] - ymean;
				xv = xv + Math.Sqrt(t1);
				yv = yv + Math.Sqrt(t2);
				s = s + t1 * t2;
			}
			if ((double)(xv) == (double)(0) | (double)(yv) == (double)(0))
			{
				result = 0;
			}
			else
			{
				result = s / (Math.Sqrt(xv) * Math.Sqrt(yv));
			}
			return result;
		}

	/*	private IndexData GetAverages(List<IndexData> values, int startFrom, int population)
		{
			IndexData averages = new IndexData();

			for (int i = startFrom; i < startFrom + population; i++)
			{
				IndexData data = values[i];
				averages.am_chg_ndx += data.am_chg_ndx;
				averages.am_chg_rty += data.am_chg_rty;
				averages.am_chg_sp += data.am_chg_sp;
			}

			averages.am_chg_ndx /= population;
			averages.am_chg_rty /= population;
			averages.am_chg_sp /= population;

			return averages;
		}*/

		private struct Stats
		{
			public double Covariance;
			public double Pearson;
			public double Variance;
		}

		// VAR = STDEV ^2

		// TODO Generalize: have lists of plain arrays...
	/*	private IndexData GetVariances(List<IndexData> indexDataList, IndexData averages, int startFrom, int population)
		{
			double sumNDX = 0;
			double sumRTY = 0;
			double sumSP = 0;

			for (int i = startFrom; i < population + startFrom; i++)
			{
				sumNDX += Math.Pow(((double)indexDataList[i].am_chg_ndx - (double)averages.am_chg_ndx), 2);
				sumRTY += Math.Pow(((double)indexDataList[i].am_chg_rty - (double)averages.am_chg_rty), 2);
				sumSP += Math.Pow(((double)indexDataList[i].am_chg_sp - (double)averages.am_chg_sp), 2);
			}

			IndexData data = new IndexData();
			data.am_chg_ndx = sumNDX / population;
			data.am_chg_rty = sumRTY / population;
			data.am_chg_sp = sumSP / population;

			return data;
		}*/




		private double GetVariance(List<CalcedValues> values, double average, int startFrom, int population)
		{
			double sum = 0;

			for (int i = startFrom; i < population + startFrom; i++)
			{
				sum += Math.Pow((values[i].am_chg - average), 2);

				//sb.AppendLine(values[i].am_chg + "\t" + average);
			}

			//				if (startFrom < 10) {
			//					sb.Append(Environment.NewLine);
			//					sb.Append(Environment.NewLine);
			//				Global.FormMain.AddText(sb.ToString());
			//				}

			return sum / population;
		}

	}

}
