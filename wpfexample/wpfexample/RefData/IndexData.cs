using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class IndexData
    {
        public int? id_imnt { get; set; }
        public string id_front_mth { get; set; }
        public string nm_front_mth { get; set; }
        public DateTime? dt_front_mth { get; set; }
        public string id_next_mth { get; set; }
        public string nm_next_mth { get; set; }
        public DateTime? dt_next_mth { get; set; }
        public string id_ae { get; set; }
        public float? am_borrow { get; set; }
        public float? am_rate_main { get; set; }
        public int? id_use_img_div { get; set; }
        public string id_und_f_c { get; set; }
        public int? id_und_mult { get; set; }
        public int? id_typ_tranche { get; set; }
        public string id_imnt_mds { get; set; }
        public float? am_div_bb { get; set; }
        public string id_imnt_activ_front { get; set; }
        public string id_imnt_activ_next { get; set; }


        public IndexData(object[] indexdataRaw)
        {
            id_imnt = (int)indexdataRaw[0];
            id_front_mth = indexdataRaw[1].ToString().Length == 0 ? null : (string)indexdataRaw[1];
            nm_front_mth = indexdataRaw[2].ToString().Length == 0 ? null : (string)indexdataRaw[2];
            dt_front_mth = indexdataRaw[3].ToString().Length == 0 ? null :  (DateTime?)indexdataRaw[3];
            id_next_mth = indexdataRaw[4].ToString().Length == 0 ? null : (string)indexdataRaw[4];
            nm_next_mth = indexdataRaw[5].ToString().Length == 0 ? null : (string)indexdataRaw[5];
            dt_next_mth = indexdataRaw[6].ToString().Length == 0 ? null :  (DateTime?)indexdataRaw[6];
            id_ae = indexdataRaw[7].ToString().Length == 0 ? null : (string)indexdataRaw[7];
            am_borrow = indexdataRaw[8].ToString().Length == 0 ? null : (float?)(double)indexdataRaw[8];
            am_rate_main = indexdataRaw[9].ToString().Length == 0 ? null : (float?)(double)indexdataRaw[9];
            id_use_img_div = (int)indexdataRaw[10];
            id_und_f_c = indexdataRaw[11].ToString().Length == 0 ? null : (string)indexdataRaw[11];
            id_und_mult = (int)indexdataRaw[12];
            id_typ_tranche = (int)indexdataRaw[13];
            id_imnt_mds = indexdataRaw[14].ToString().Length == 0 ? null : (string)indexdataRaw[14];
            am_div_bb = indexdataRaw[15].ToString().Length == 0 ? null : (float?)(double)indexdataRaw[15];
            id_imnt_activ_front = indexdataRaw[16].ToString().Length == 0 ? null : (string)indexdataRaw[16];
            id_imnt_activ_next = indexdataRaw[17].ToString().Length == 0 ? null : (string)indexdataRaw[17];


        }
    }
}