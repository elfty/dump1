using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YJ.AppLink
{
    public class WETheo
    {
        public DateTime dt_bus;
        public string id_imnt_ric { get; set; }
        public DateTime dt_mat { get; set; }
        public float pr_strike { get; set; }
        public string id_pc { get; set; }
        public float pr_stk { get; set; }
        public float pr_bid { get; set; }
        public float pr_ask { get; set; }
        public float pr_theo { get; set; }
        public float am_delta { get; set; }
        public float am_gamma { get; set; }
        public float am_vega { get; set; }
        public float am_theta { get; set; }
        public float am_vol { get; set; }
        
    }
}
