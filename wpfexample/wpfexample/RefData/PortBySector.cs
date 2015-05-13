using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wpfexample
{
    public class PortBySector
    {
        public string nm_gics_sector { get; set; }
        public float? am_gamma { get; set; }
        public float? am_vega { get; set; }
        public float? am_vega_3m { get; set; }
        public float? am_theta { get; set; }
    }
}
