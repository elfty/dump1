using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{

    class VolData
    {
        public DateTime dt_bus { get; set; }
        public int? id_imnt { get; set; }
        public float? pr_close { get; set; }
        public float? pr_open { get; set; }
        public float? am_chg { get; set; }
        public float? am_ivol_1m_95 { get; set; }
        public float? am_ivol_1m_100 { get; set; }
        public float? am_ivol_1m_105 { get; set; }
        public float? am_ivol_2m_95 { get; set; }
        public float? am_ivol_2m_100 { get; set; }
        public float? am_ivol_2m_105 { get; set; }
        public float? am_ivol_3m_95 { get; set; }
        public float? am_ivol_3m_100 { get; set; }
        public float? am_ivol_3m_105 { get; set; }

        public VolData(object[] voldataRaw)
        {
            dt_bus = (DateTime)voldataRaw[0];
            id_imnt = (int)voldataRaw[1];
            pr_close = voldataRaw[2].ToString().Length == 0 ? null : (float?)(double)voldataRaw[2];
            pr_open = voldataRaw[3].ToString().Length == 0 ? null : (float?)(double)voldataRaw[3];
            am_chg = voldataRaw[4].ToString().Length == 0 ? null : (float?)(double)voldataRaw[4];
            am_ivol_1m_95 = voldataRaw[5].ToString().Length == 0 ? null : (float?)(double)voldataRaw[5];
            am_ivol_1m_100 = voldataRaw[6].ToString().Length == 0 ? null : (float?)(double)voldataRaw[6];
            am_ivol_1m_105 = voldataRaw[7].ToString().Length == 0 ? null : (float?)(double)voldataRaw[7];
            am_ivol_2m_95 = voldataRaw[8].ToString().Length == 0 ? null : (float?)(double)voldataRaw[8];
            am_ivol_2m_100 = voldataRaw[9].ToString().Length == 0 ? null : (float?)(double)voldataRaw[9];
            am_ivol_2m_105 = voldataRaw[10].ToString().Length == 0 ? null : (float?)(double)voldataRaw[10];
            am_ivol_3m_95 = voldataRaw[11].ToString().Length == 0 ? null : (float?)(double)voldataRaw[11];
            am_ivol_3m_100 = voldataRaw[12].ToString().Length == 0 ? null : (float?)(double)voldataRaw[12];
            am_ivol_3m_105 = voldataRaw[13].ToString().Length == 0 ? null : (float?)(double)voldataRaw[13];
        }
    }
}
