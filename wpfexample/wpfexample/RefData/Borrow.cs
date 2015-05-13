using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class Borrow
    {
        public int? id_imnt { get; set; }
        public string id_imnt_reuters { get; set; }
        public string id_cusip { get; set; }
        public string id_imnt_ric { get; set; }
        public float? am_shares_max { get; set; }
        public float? am_rate { get; set; }

        public Borrow(object[] borrowRaw)
        {
            id_imnt = borrowRaw[0].ToString().Length == 0 ? null : (int?)borrowRaw[0];
            id_imnt_reuters = (string)borrowRaw[1];
            id_cusip = (string)borrowRaw[2];
            id_imnt_ric = (string)borrowRaw[3];
            am_shares_max = (float)(double)borrowRaw[4];
            am_rate = (float)(double)borrowRaw[5];
        }
    }
}