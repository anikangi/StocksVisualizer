using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksVisualizerUI
{
    public class DateComparer : IEqualityComparer<MinuteData>
    {
        public bool Equals(MinuteData x, MinuteData y)
        {
            if (x.StockID == y.StockID)
            {
                return true;
            }
            return false;
        }
        //public bool Equals(MinuteData x, MinuteData y)
        //{
        //    if (x.StockID == y.StockID && x.Date.Year == y.Date.Year && x.Date.Month == y.Date.Month && x.Date.Day == y.Date.Day)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        public int GetHashCode(MinuteData obj)
        {
            return obj.ID.GetHashCode();
        }
    }
}
