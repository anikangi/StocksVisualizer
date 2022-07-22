using Nager.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksVisualizerUI
{
    /// <summary>
    /// Contains all methods for calculating date ranges excluding non-business days.
    /// </summary>
    public static class BusinessDateRange //returns xml comments guidelines???
    {
        /// <summary>
        /// Returns a new <c>List<DateTime></c> of business days ranging from the specified start date to (but not including) the end date.
        /// <param name="start">The start date for the range.</param>
        /// <param name="end">The end date for the range, NOT included in range.</param>
        /// </summary>


        public static List<DateTime> DatesBetween(DateTime start, DateTime end, List<DateTime> holidays)
        {
            var full_daterange = Enumerable.Range(0, end.Subtract(start).Days).Select(d => start.AddDays(d));
            var daterange = new List<DateTime>();
            foreach (var date in full_daterange)
            {
                if (!DateSystem.IsWeekend(date, CountryCode.US) && !holidays.Contains(date))
                {
                    daterange.Add(date);
                }
            }
            return daterange;

        }

        /// <summary>
        /// Returns a new <c>List<DateTime></c> of the specified length of business days before the specified date.
        /// </summary>
        /// <param name="date">The end date, NOT included in range.</param>
        /// <param name="count">The amount of days in the daterange.</param>

        public static List<DateTime> DatesBefore(DateTime date, int count, List<DateTime> holidays)
        {
            var daterange = new List<DateTime>();
            int d = 1;
            while (daterange.Count < count)
            {
                var current_date = date.AddDays(-d);
                d++;
                if (!DateSystem.IsWeekend(current_date, CountryCode.US) && !holidays.Contains(current_date))
                {
                    daterange.Add(current_date);
                }
            }

            return daterange;
        }

        /// <summary>
        /// Returns a new <c>List<DateTime></c> of the specified length of business days after the specified date.
        /// </summary>
        /// <param name="date">The start date, NOT included in range.</param>
        /// <param name="count">The amount of days in the daterange.</param>

        public static List<DateTime> DatesAfter(DateTime date, int count, List<DateTime> holidays)
        {
            var daterange = new List<DateTime>();
            int d = 1;
            while (daterange.Count < count)
            {
                var current_date = date.AddDays(d);
                d++;
                if (!DateSystem.IsWeekend(current_date, CountryCode.US) && !holidays.Contains(current_date))
                {
                    daterange.Add(current_date);
                }
            }

            return daterange;
        }

    }
}
