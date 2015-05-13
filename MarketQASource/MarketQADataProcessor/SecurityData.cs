using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal class SecurityData
	{
		readonly object[] _rawData;

		static readonly int NumOfFields = Enum.GetNames(typeof(Clm)).Length;

		public SecurityData()
		{
			// HACK to initialize the first missing entry for a security
			_rawData = new object[]
			           	{
			           		0,
							DateTime.MinValue,
							0.0f,
							0.0f,
							0.0f,
							0,
							0.0f,
							0.0f,
							0.0f,
							0,
							0.0f
						};
		}

		public SecurityData(object[] rawData)
		{
			_rawData = rawData;
		}

		public object[] RawData
		{
			get { return _rawData; }
		}

		public int SecurityID
		{
			get
			{
				return (int)_rawData[(int)Clm.SecurityID];
			}
		}

		public DateTime Date
		{
			get
			{
				return (DateTime)_rawData[(int)Clm.Date];
			}
			set
			{
				_rawData[(int)Clm.Date] = value;
			}
		}


		public double BidLow
		{
			get
			{
				return (float)_rawData[(int)Clm.BidLow];
			}
		}

		public double AskHigh
		{
			get
			{
				return (float)_rawData[(int)Clm.AskHigh];
			}
		}

		public double ClosePrice
		{
			get
			{
				return (float)_rawData[(int)Clm.ClosePrice];
			}
		}

		public double Volume
		{
			get
			{
				return (float)_rawData[(int)Clm.Volume];
			}
		}

		public double TotalReturn
		{
			get
			{
				return (float)_rawData[(int)Clm.TotalReturn];
			}
		}

		public int AdjustmentFactor
		{
			get
			{
				return (int)_rawData[(int)Clm.AdjustmentFactor];
			}
		}

		public double OpenPrice
		{
			get
			{
				return (float)_rawData[(int)Clm.OpenPrice];
			}
		}

		public int SharesOutstanding
		{
			get
			{
				return (int)_rawData[(int)Clm.SharesOutstanding];
			}
		}

		public double AdjustmentFactor2
		{
			get
			{
				return (float)_rawData[(int)Clm.AdjustmentFactor2];
			}
		}
	}
}
