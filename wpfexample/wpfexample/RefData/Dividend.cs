using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class Dividend
    {
        public int? id_imnt { get; set; }
        public string id_imnt_ric { get; set; }
        public string id_src { get; set; }
        public DateTime? dt_ex { get; set; }
        public float? am_div { get; set; }
        public DateTime? dt_chg { get; set; }
        public string id_freq { get; set; }
        public string tx_status { get; set; }
        public string tx_proj { get; set; }

        public Dividend(object[] divRaw)
        {
            id_imnt = (int)divRaw[0];
            id_imnt_ric = (string)divRaw[1];
            id_src = (string)divRaw[2];
            dt_ex = (DateTime)divRaw[3];
            am_div = (float)(double)divRaw[4];
            dt_chg = divRaw[5].ToString().Length == 0 ? null : (DateTime?)divRaw[5];
            id_freq = divRaw[6].ToString().Length == 0 ? null : (string)divRaw[6];
            tx_status = divRaw[7].ToString().Length == 0 ? null : (string)divRaw[7];
            tx_proj = divRaw[8].ToString().Length == 0 ? null : (string)divRaw[8];
        }
    }
}