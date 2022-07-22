using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;


namespace Normalization
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var test_data = new List<double>();
                using (var context = new NeeksDBEntities())
                {
                    test_data = context.HistoricalDatas.Select(x => x.Close).Take(50).ToList();
                }

                var mean = test_data.Average();
                var sd = test_data.StandardDev();
                if (sd == 0)
                {
                    Console.WriteLine("Denominator(standard deviation) cannot = 0");
                    return;
                }
                var standardized_data = new List<double>();

                var min = test_data.Min();
                var max = test_data.Max();
                if(max - min == 0)
                {
                    Console.WriteLine("Difference between max and min value cannot = 0");
                    return;
                }
                var normalized_data = new List<double>();


                for (int i = 0; i < test_data.Count; i++)
                {
                    var n = Normalizations.NormalizeVal(test_data[i], min, max);
                    normalized_data.Add(n);

                    var s = Normalizations.StandardizeVal(test_data[i], sd, mean);
                    standardized_data.Add(s);
                }

                Console.WriteLine("Original Values:");
                Console.WriteLine(string.Join(",", test_data));
                Console.WriteLine();

                Console.WriteLine("Standardized Values:");
                Console.WriteLine(string.Join(",", standardized_data));
                Console.WriteLine();

                Console.WriteLine("Normalized Values:");
                Console.WriteLine(string.Join(",", normalized_data));
                Console.WriteLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadLine();
            }
            
        }        
    }
}
