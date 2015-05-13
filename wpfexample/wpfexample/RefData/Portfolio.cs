using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    public class Portfolio
    {
        public int? id_prtf { get; set; }
        public string nm_prtf { get; set; }
        public int? id_imnt_und { get; set; }
        public float? pr_und { get; set; }
        public float? am_vol { get; set; }
        public float? am_freq { get; set; }
        public float? pr_lvl { get; set; }
        public float? am_delta_lvl { get; set; }
        public float? pr_lvl_lo { get; set; }
        public float? am_lvl_lo { get; set; }
        public float? pr_lvl_hi { get; set; }
        public float? am_lvl_hi { get; set; }
        public float? am_delta { get; set; }
        public float? am_pos_stk { get; set; }
        public float? am_pos_stk_day { get; set; }
        public float? am_pl { get; set; }
        public string tx_note { get; set; }
        public int? id_strat { get; set; }
        public DateTime? dt_val { get; set; }
        public int? id_auto { get; set; }
        public int? id_ord_buy { get; set; }
        public int? id_ord_sell { get; set; }
        public int? id_ord_partial { get; set; }
        public float? pr_und_close { get; set; }

        public Portfolio(object[] prtfRaw)
        {
            id_prtf = prtfRaw[0].ToString().Length == 0 ? null : (int?)prtfRaw[0];
            nm_prtf = prtfRaw[1].ToString().Length == 0 ? null : (string)prtfRaw[1];
            id_imnt_und = prtfRaw[2].ToString().Length == 0 ? null : (int?)prtfRaw[2];
            pr_und = prtfRaw[3].ToString().Length == 0 ? null : (float?)(double)prtfRaw[3];
            am_vol = prtfRaw[4].ToString().Length == 0 ? null : (float?)(double)prtfRaw[4];
            am_freq = prtfRaw[5].ToString().Length == 0 ? null : (float?)(double)prtfRaw[5];
            pr_lvl = prtfRaw[6].ToString().Length == 0 ? null : (float?)(double)prtfRaw[6];
            am_delta_lvl = prtfRaw[7].ToString().Length == 0 ? null : (float?)(double)prtfRaw[7];
            pr_lvl_lo = prtfRaw[8].ToString().Length == 0 ? null : (float?)(double)prtfRaw[8];
            am_lvl_lo = prtfRaw[9].ToString().Length == 0 ? null : (float?)(double)prtfRaw[9];
            pr_lvl_hi = prtfRaw[10].ToString().Length == 0 ? null : (float?)(double)prtfRaw[10];
            am_lvl_hi = prtfRaw[11].ToString().Length == 0 ? null : (float?)(double)prtfRaw[11];
            am_delta = prtfRaw[12].ToString().Length == 0 ? null : (float?)(double)prtfRaw[12];
            am_pos_stk = prtfRaw[13].ToString().Length == 0 ? null : (float?)(double)prtfRaw[13];
            am_pos_stk_day = prtfRaw[14].ToString().Length == 0 ? null : (float?)(double)prtfRaw[14];
            am_pl = prtfRaw[15].ToString().Length == 0 ? null : (float?)(double)prtfRaw[15];
            tx_note = prtfRaw[16].ToString().Length == 0 ? null : (string)prtfRaw[16];
            id_strat = prtfRaw[17].ToString().Length == 0 ? null : (int?)prtfRaw[17];
            dt_val = prtfRaw[18].ToString().Length == 0 ? null : (DateTime?) (prtfRaw[18]);
            id_auto = prtfRaw[19].ToString().Length == 0 ? null : (int?)prtfRaw[19];
            id_ord_buy = prtfRaw[20].ToString().Length == 0 ? null : (int?)prtfRaw[20];
            id_ord_sell = prtfRaw[21].ToString().Length == 0 ? null : (int?)prtfRaw[21];
            id_ord_partial = prtfRaw[22].ToString().Length == 0 ? null : (int?)prtfRaw[22];
            pr_und_close = prtfRaw[23].ToString().Length == 0 ? null : (float?)(double)prtfRaw[23];
        }
    }
}
