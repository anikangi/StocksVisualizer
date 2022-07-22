using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksVisualizerUI
{
    public class UIStockInfo
    {
        //need to add Disabled & percent data available properties
        public long ID { get; set; }
        public string Symbol { get; set; }
        public string Company { get; set; }
        public string Exchange { get; set; }
        public string Industry { get; set; }
        public string Sector { get; set; }
        public string Description { get; set; }
        public bool IsStockChecked { get; set; }
        public string MostRecentDate_Daily { get; set; }
        public string LastUpdated_Minute { get; set; }
        public bool IsDisabled { get; set; }
        public long? MarketCap { get; set; }
        public DateTime? Last_Updated_MC { get; set; }
        public string MostRecentDate_Stats { get; set; }
        //public bool IsSelected { get; set; }

    }

}
