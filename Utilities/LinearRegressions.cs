using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtensionMethods;

namespace Utilities
{
    public static class LinearRegressions
    {
        //this method: must allow for decimals
        public static void LinReg(double[] x_vals, double[] y_vals,
            out double slope, out double y_int, out double r, out double r_squared, out double stDev_residuals)
        {
            //slope:
            var xy_pts = Enumerable.Zip(x_vals, y_vals, (x, y) => new { x = x, y = y });
            double xbar = x_vals.Average();
            double ybar = y_vals.Average();
            slope = xy_pts.Sum(xy => (xy.x - xbar) * (xy.y - ybar)) / x_vals.Sum(x => Math.Pow((x - xbar), 2));

            //y-int:
            y_int = ybar - (slope * xbar);

            //r: 
            double x_stDev = x_vals.StandardDev();
            double y_stDev = y_vals.StandardDev();

            r = slope * (x_stDev / y_stDev);

            //r_squared:
            r_squared = Math.Pow(r, 2);


            //stDev_residuals:
            var y_hat_pts = GeneratePredictedPts(x_vals, slope, y_int).Select(p => p[1]);

            var n = y_vals.Length;
            if (n != y_hat_pts.Count())
            {
                stDev_residuals = 0;
                return;
            }

            var both_ys = Enumerable.Zip(y_vals, y_hat_pts, (y, y_hat) => new { y = y, y_hat = y_hat });
            var numerator = both_ys.Sum(v => Math.Pow((v.y - v.y_hat), 2));
            stDev_residuals = Math.Sqrt(numerator / (n - 2));

        }


        public static double[][] GeneratePredictedPts(double[] x_vals, double slope, double y_int)
        {
            double y;
            double[][] predicted_vals = new double[x_vals.Length][];
            for (int i = 0; i < x_vals.Length; i++)
            {
                y = y_int + (slope * x_vals[i]);
                var xy_pair = new double[] { x_vals[i], y };
                predicted_vals[i] = xy_pair;
            }
            return predicted_vals;
        }

        public static double SlopeToDegrees(double slope)
        {
            var val = 0d;
                var radians = Math.Atan(slope);
                val = radians * (180 / Math.PI);
            return val;

        }
    }

}
