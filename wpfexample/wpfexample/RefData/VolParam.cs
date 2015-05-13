using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class VolParam
    {
        public int id_imnt { get; set; }
        public DateTime dt_mat { get; set; }
        public float am_vol_atm { get; set; }
        public float am_skew_dn { get; set; }
        public float am_skew_up { get; set; }
        public float pr_und { get; set; }
        public float am_min { get; set; }
        public float am_max { get; set; }
        public DateTime dt_chg { get; set; }
        public string id_chg { get; set; }
        public int skew_ts { get; set; }


        public VolParam(object[] volparamRaw)
        {
            id_imnt = (int) volparamRaw[0];
            dt_mat = (DateTime) volparamRaw[1];
            am_vol_atm = (float)(double) volparamRaw[2];
            am_skew_dn = (float)(double)volparamRaw[3];
            am_skew_up = (float)(double)volparamRaw[4];
            pr_und = (float)(double)volparamRaw[5];
            am_min = (float)(double)volparamRaw[6];
            am_max = (float)(double)volparamRaw[7];
            dt_chg = (DateTime) volparamRaw[8];
            id_chg = (string) volparamRaw[9];
            skew_ts = volparamRaw[10].ToString().Length == 0 ? 0 : (int) volparamRaw[10];
           

            //

        }
    }
}
