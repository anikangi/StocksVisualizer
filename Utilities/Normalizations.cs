using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Normalizations
    {
        public static double NormalizeVal(double point, double min, double max )
        {

            double denom = max - min;
            if (denom == 0)
            {
                throw new DivideByZeroException("Difference between max and min value cannot = 0.");
            }
            double x = (point - min) / denom;
            return x;


        }

        public static double StandardizeVal(double point, double sd, double mean)
        {
            if (sd == 0)
            {
                throw new DivideByZeroException("Denominator (standard deviation) cannot = 0");
            }
            double x = (point - mean) / sd;
            return x;
        }

        public static double ConvertRange(double val, double inRangeMin, double inRangeMax, double outRangeMin, double outRangeMax)
        {
            if (outRangeMin == outRangeMax)
            {
                throw new Exception("ConvertRange Error: Zero output  range");
            }

            if (inRangeMin == inRangeMax)
            {
                return 0;
            }

            // check reversed input range
            var reverseInput = false;
            var oldMin = Math.Min(inRangeMin, inRangeMax);
            var oldMax = Math.Max(inRangeMin, inRangeMax);
            if (oldMin != inRangeMin)
                reverseInput = true;


            // check reversed output range
            var reverseOutput = false;
            var newMin = Math.Min(outRangeMin, outRangeMax);
            var newMax = Math.Max(outRangeMin, outRangeMax);
            if (newMin != outRangeMin)
                reverseOutput = true;

            var portion = (reverseInput) ? (oldMax - val) * (newMax - newMin) / (oldMax - oldMin) : (val - oldMin) * (newMax - newMin) / (oldMax - oldMin);

            return (reverseOutput) ? newMax - portion : portion + newMin;


        }
    }
}
