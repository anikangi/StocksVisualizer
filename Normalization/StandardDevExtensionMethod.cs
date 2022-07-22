using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Normalization
{
    public static class StandardDevExtensionMethod
    {
        public static double StandardDev(this IEnumerable<double> vals, bool IsSamplePop = false)
        {
            var n = vals.Count();
            var mean = vals.Average();
            if (IsSamplePop) { n -= 1; }
            var temp = new List<double>();
            foreach (var val in vals)
            {
                temp.Add(Math.Pow((val - mean), 2));
            }
            //val - mean, square it, add to list, get sum of list, divide by N, take the sqrt
            var result = Math.Sqrt((temp.Sum()) / n);
            return result;
        }
    }
}
