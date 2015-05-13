using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    class PortfolioDisplay
    {
        public int? id_prtf { get; set; }
        public int? prtf_ct { get; set; }
        public string nm_prtf { get; set; }
        public float? pr_und { get; set; }
        public float? am_pct_chg { get; set; }
        public float? am_delta { get; set; }
        public float? am_delta_abs { get; set; }
        public float? am_delta_ct { get; set; }
        public float? am_gamma { get; set; }
        public float? am_vega { get; set; }
        public float? am_theta { get; set; }
        public float? am_pl { get; set; }
        public float? am_vega15 { get; set; }
        public float? am_vegavol15 { get; set; }
        public float? am_impdelta { get; set; }
        public float? am_pl_trd { get; set; }
        public float? am_pl_day { get; set; }
        public float? am_edge { get; set; }
    }
}
