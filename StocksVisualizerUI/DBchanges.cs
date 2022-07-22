using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StocksVisualizerUI
{
    public static class DBchanges
    {
        public static void Update_MarketCaps(List<MarketCapitalization> marketCaps)
        {
            using (var context = new NeeksDBEntities())
            {
                if (context.MarketCapitalizations.Any())
                {
                    marketCaps.ForEach(cap =>
                    {
                        var rec = context.MarketCapitalizations.FirstOrDefault(x => x.StockID == cap.StockID);
                        if (rec != null)
                        {
                            if (cap.MarketCap != 0)
                            {
                                rec.MarketCap = cap.MarketCap;
                            }
                            rec.Date = cap.Date;
                        }
                        else
                        {
                            context.MarketCapitalizations.Add(cap);

                        }
                    });
                }
                else
                {
                    context.MarketCapitalizations.AddRange(marketCaps);
                }
                context.SaveChanges();
            }
        }

        public static void Insert_HistDatas(List<HistoricalData> API_data)
        {
            using (var context = new NeeksDBEntities())
            {
                //await Task.Delay(200);
                context.HistoricalDatas.AddRange(API_data);
                context.SaveChanges();
            }
        }

        public static void Insert_MinuteDatas(List<MinuteData> db_data)
        {
            using (var context = new NeeksDBEntities())
            {
                //await Task.Delay(200);
                context.MinuteDatas.AddRange(db_data);
                //context.MinuteDataTesters.AddRange(db_data);
                context.SaveChanges();
            }
        }

        public static void Insert_Stats(List<SampleStat> sample_stats)
        {
            using (var context = new NeeksDBEntities())
            {
                context.SampleStats.AddRange(sample_stats);
                context.SaveChanges();
            }
        }

        //public static void Update_MinDataColumn_In_Stock(List<long> stockIDs)
        //{
        //    using (var context = new NeeksDBEntities())
        //    {
        //        if (context.Stocks.Any())
        //        {
        //            stockIDs.ForEach(id =>
        //            {
        //                var rec = context.Stocks.FirstOrDefault(x => x.ID == id);
        //                if (rec != null)
        //                {
        //                    rec.LastUpdated_MinData = DateTime.Today;
        //                }
        //            });
        //        }
        //        context.SaveChanges();
        //    }
        //}

        public static void Update_IsDisabledColumn_In_Stock(long id, bool val)
        {
            using (var context = new NeeksDBEntities())
            {
                var rec = context.Stocks.FirstOrDefault(x => x.ID == id);
                if (rec != null)
                {
                    rec.IsDisabled = val;
                }
                context.SaveChanges();
            }
        }
        public static void Update_MinDataColumns_In_Stock(long stockID, decimal PercentData_LastDay_val, decimal PercentData_SamplePeriod_val)
        {
            using (var context = new NeeksDBEntities())
            {
                var rec = context.Stocks.FirstOrDefault(x => x.ID == stockID);
                if (rec != null)
                {
                    rec.LastUpdated_MinData = DateTime.Today;
                    rec.PercentData_LastDay = PercentData_LastDay_val;
                    rec.PercentData_SamplePeriod = PercentData_SamplePeriod_val;
                }
                context.SaveChanges();
            }
        }


        //public static void Update_PercentData_SamplePeriodColumn_In_Stock(long id, int period)
        //{
        //    using (var context = new NeeksDBEntities())
        //    {
        //        //var temp = context.MinuteDatas.Where(x => x.StockID == id);
        //        //var to = temp.Max(x => x.Date);
        //        var earlyCloseDates = context.Holidays.Where(x => x.EarlyClose).Select(x => x.Date).ToList();

        //        var daterange = new List<DateTime>(context.GetDateRangeForStock(period, id).Select(x=> x.Value));
        //        var from = daterange.Min();
        //        var to = daterange.Max().AddDays(1);
        //        var nullRows = context.MinuteDatas.Where(x => x.StockID == id).Where(x => x.Date >= from && x.Date < to).Where(x=> x.Volume == 0).Count();
        //        var daysEarlyClose = (earlyCloseDates.Intersect(daterange)).Count();

        //        var val = nullRows / ((240*daysEarlyClose) + (390*(period-daysEarlyClose))); //make sure this is a decimal 
        //        var rec = context.Stocks.FirstOrDefault(x => x.ID == id);
        //        if (rec != null)
        //        {
        //            rec.PercentData_SamplePeriod = val;
        //        }
        //        context.SaveChanges();
        //    }

        //}

    }
}
