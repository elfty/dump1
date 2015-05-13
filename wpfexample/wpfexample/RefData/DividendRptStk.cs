using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class DividendRptStk
    {
        public DateTime dt_trd { get; set; }
        public int id_pos { get; set; }
        public int id_prtf { get; set; }
        public int id_imnt { get; set; }
        public float am_pos { get; set; }
        public float am_pos_adj { get; set; }
        public float pr_imnt { get; set; }
        public float pr_imnt_close { get; set; }
        public float pr_theo { get; set; }
        public float pr_und { get; set; }
        public float am_pl { get; set; }
        public float am_pl_sum { get; set; }
        public float am_pl_nie { get; set; }
        public float am_pl_nie_sum { get; set; }
        public float am_vol { get; set; }
        public float am_vol_or { get; set; }
        public float am_vol_imp { get; set; }
        public float am_vol_close { get; set; }
        public float am_vol_mark { get; set; }
        public float am_vol_hedge { get; set; }
        public float am_delta { get; set; }
        public float am_gamma { get; set; }
        public float am_vega { get; set; }
        public float am_theta { get; set; }
        public float am_rho { get; set; }
        public float am_time { get; set; }
        public float am_rate { get; set; }


        public float am_delta_imp { get; set; }
        public float pr_trd { get; set; } //not from POSITIONS table
        public float am_pl_day { get; set; } //not from POSITIONS table
        public string id_src { get; set; }
        public DateTime? dt_ex { get; set; }
        public float? am_div { get; set; }
        public DateTime? dt_chg { get; set; }
        public string id_freq { get; set; }
        public string tx_status { get; set; }
        public string tx_proj { get; set; }

        public DividendRptStk(object[] positionRaw)
        {
            dt_trd = (DateTime)positionRaw[0];
            id_pos = (int)positionRaw[1];
            id_prtf = (int)positionRaw[2];
            id_imnt = (int)positionRaw[3];
            am_pos = (float)(double)positionRaw[4];
            am_pos_adj = (float)(double)positionRaw[5];
            pr_imnt = (float)(double)positionRaw[6];
            pr_imnt_close = (float)(double)positionRaw[7];
            pr_theo = (float)(double)positionRaw[8];
            pr_und = (float)(double)positionRaw[9];
            am_pl = (float)(double)positionRaw[10];
            am_pl_sum = (float)(double)positionRaw[11];
            am_pl_nie = (float)(double)positionRaw[12];
            am_pl_nie_sum = (float)(double)positionRaw[13];
            am_vol = (float)(double)positionRaw[14];
            am_vol_or = (float)(double)positionRaw[15];
            am_vol_imp = (float)(double)positionRaw[16];
            am_vol_close = (float)(double)positionRaw[17];
            am_vol_mark = (float)(double)positionRaw[18];
            am_vol_hedge = (float)(double)positionRaw[19];
            am_delta = (float)(double)positionRaw[20];
            am_gamma = (float)(double)positionRaw[21];
            am_vega = (float)(double)positionRaw[22];
            am_theta = (float)(double)positionRaw[23];
            am_rho = (float)(double)positionRaw[24];
            am_time = (float)(double)positionRaw[25];
            am_rate = (float)(double)positionRaw[26];

            dt_chg = (DateTime)positionRaw[28];
            id_src = (string)positionRaw[31];
            dt_ex = (DateTime)positionRaw[32];
            am_div = (float)(double)positionRaw[33];

            id_freq = positionRaw[35].ToString().Length == 0 ? null : (string)positionRaw[35];
            tx_status = positionRaw[36].ToString().Length == 0 ? null : (string)positionRaw[36];
            tx_proj = positionRaw[37].ToString().Length == 0 ? null : (string)positionRaw[37];

        }

    }
}
