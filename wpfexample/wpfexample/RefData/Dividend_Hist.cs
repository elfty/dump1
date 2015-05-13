using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class Dividend_Hist
    {
        public int? id_imnt { get; set; }
        public string id_imnt_ric { get; set; }
        public DateTime? dt_ex { get; set; }
        public float? am_div { get; set; }


        public Dividend_Hist(object[] divRaw)
        {
            id_imnt = (int)divRaw[0];
            id_imnt_ric = (string)divRaw[1];
            dt_ex = (DateTime)divRaw[2];
            am_div = (float)(double)divRaw[3];
        }
    }
}
