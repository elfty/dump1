using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class RatesHist
    {
        public string id_rate { get; set; }
        public float am_rate { get; set; }
        public float am_yrs { get; set; }
        public DateTime dt_chg { get; set; }
        public int am_days { get; set; }

        public RatesHist(object[] rateshistRaw)
        {
            id_rate = (string)rateshistRaw[0];
            am_rate = (float)(double)rateshistRaw[1];
            am_yrs = (float)rateshistRaw[2];
            dt_chg = (DateTime)rateshistRaw[3];
            am_days = (int)rateshistRaw[4];
        }
    }
}
