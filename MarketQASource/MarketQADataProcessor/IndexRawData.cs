using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketQADataProcessor
{
	internal class IndexRawData
	{
		readonly object[] _rawData;

		public IndexRawData(object[] rawData)
		{
			_rawData = rawData;
		}

		public object[] RawData
		{
			get { return _rawData; }
		}

		public DateTime Date
		{
			get
			{
				return (DateTime)_rawData[(int)Clm.Date];
			}
		}

		public int SecurityID
		{
			get
			{
				return (int)_rawData[(int)Clm.SecurityID];
			}
		}

		public double? Value
		{
			get
			{
				return (_rawData[(int)Clm.Value] == DBNull.Value) ? null : (float?)_rawData[(int)Clm.Value];
			}
		}

		private enum Clm
		{
			Date,
			SecurityID,
			Value
		}
	}
}