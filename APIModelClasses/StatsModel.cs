using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIModelClasses
{
    public class Stats
    {
        public long? marketCap { get; set; }
    }

    public class IEXStock_MC
    {
        public Stats stats { get; set; }
    }
}
