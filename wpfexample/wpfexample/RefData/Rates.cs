using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class Rates
    {
        public string id_rate { get; set; }
        public float am_rate { get; set; }
        public float am_yrs { get; set; }
        public DateTime dt_chg { get; set; }
        public int am_days { get; set; }

        public Rates(object[] divRaw)
        {
            id_rate = (string) divRaw[0];
            am_rate = (float)(double) divRaw[1];
            am_yrs = (float)divRaw[2];
            dt_chg = (DateTime) divRaw[3];
            am_days = (int) divRaw[4];
        }
    }
}
