using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace APIModelClasses
{
    public class MinuteDataModel
    {
        public DateTime date { get; set; }
        public string minute { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? marketAverage { get; set; }
        public long? marketVolume { get; set; }
        public long? marketNumberOfTrades { get; set; }
    }
}
