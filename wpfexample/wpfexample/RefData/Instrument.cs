using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{

    public class Instrument
    {
        public int? id_imnt { get; set; }
        public string nm_imnt { get; set; }
        public string id_imnt_ric { get; set; }
        public string id_imnt_imagine { get; set; }
        public string id_cusip { get; set; }
        public string id_typ_imnt { get; set; }
        public string id_exch { get; set; }
        public int? id_gics_sector { get; set; }
        public int? id_gics_industry_sub { get; set; }
        public string id_opt_actv { get; set; }
        public float? am_ct_sz { get; set; }
        public float? am_mult { get; set; }
        public int? id_imnt_und { get; set; }
        public float? pr_strike { get; set; }
        public DateTime? dt_mat { get; set; }
        public string id_pc { get; set; }
        public int? id_ae { get; set; }
        public string id_imnt_reuters { get; set; }
        public int? id_imnt_ivy { get; set; }
        public string id_bberg { get; set; }
        public string id_div_src { get; set; }
        public DateTime? dt_chg { get; set; }
        public int? id_del { get; set; }
        public string id_crt { get; set; }

        public Instrument(object[] instRaw)
        {
            id_imnt = (int)instRaw[0];
            nm_imnt = instRaw[1].ToString().Length == 0 ? null : (string)instRaw[1];
            id_imnt_ric = instRaw[2].ToString().Length == 0 ? null : (string)instRaw[2];
            id_imnt_imagine = instRaw[3].ToString().Length == 0 ? null : (string)instRaw[3];
            id_cusip = instRaw[4].ToString().Length == 0 ? null : (string)instRaw[4];
            id_typ_imnt = instRaw[5].ToString().Length == 0 ? null : (string)instRaw[5];
            id_exch = instRaw[6].ToString().Length == 0 ? null : (string)instRaw[6];
            id_gics_sector = instRaw[7].ToString().Length == 0 ? null : (int?)instRaw[7];
            id_gics_industry_sub = instRaw[8].ToString().Length == 0 ? null : (int?)instRaw[8];
            id_opt_actv = instRaw[9].ToString().Length == 0 ? null : (string)instRaw[9];
            am_ct_sz = instRaw[10].ToString().Length == 0 ? null : (float?)(double)instRaw[10];
            am_mult = instRaw[11].ToString().Length == 0 ? null : (float?)(double)instRaw[11];
            id_imnt_und = (int)instRaw[12];
            pr_strike = instRaw[13].ToString().Length == 0 ? null : (float?)(double)instRaw[13];
            dt_mat = instRaw[14].ToString().Length == 0 ? null : (DateTime?)instRaw[14];
            id_pc = instRaw[15].ToString().Length == 0 ? null : (string)instRaw[15];
            id_ae = instRaw[16].ToString().Length == 0 ? null : (int?)instRaw[16];
            id_imnt_reuters = instRaw[17].ToString().Length == 0 ? null : (string)instRaw[17];
            id_imnt_ivy = instRaw[18].ToString().Length == 0 ? null : (int?)instRaw[18];
            id_bberg = instRaw[19].ToString().Length == 0 ? null : (string)instRaw[19];
            id_div_src = instRaw[20].ToString().Length == 0 ? null : (string)instRaw[20];
            dt_chg = instRaw[21].ToString().Length == 0 ? null : (DateTime?)instRaw[21];
            id_del = instRaw[22].ToString().Length == 0 ? null : (int?)instRaw[22];
            id_crt = instRaw[23].ToString().Length == 0 ? null : (string)instRaw[23];

        }
    }
}
