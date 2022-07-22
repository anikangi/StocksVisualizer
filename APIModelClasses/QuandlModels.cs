using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIModelClasses
{
    public class Rootobject
    {
        public Datatable datatable { get; set; }
        public Meta meta { get; set; }
    }

    public class Meta
    {
        public object next_cursor_id { get; set; }
    }

    public class Datatable
    {
        public object[][] data { get; set; }
        public Column[] columns { get; set; }
    }

    public class Column
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class QuandlDailyMetrics
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastUpdated { get; set; }
        public double Ev { get; set; }
        public double Evebit { get; set; }
        public double Evebitda { get; set; }
        public double MarketCap { get; set; }
        public double Pb { get; set; }
        public double Pe { get; set; }
        public double Ps { get; set; }
    }
}
