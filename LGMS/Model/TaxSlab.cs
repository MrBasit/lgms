using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LGMS.Model
{
    public class TaxSlab
    {
        public decimal Min { get; set; }
        public decimal? Max { get; set; } // nullable for upper limit
        public decimal Rate { get; set; } // as percentage e.g., 0.05 for 5%
    }

    public class IncomeTaxSettings
    {
        public List<TaxSlab> Slabs { get; set; } = new();
    }
}
