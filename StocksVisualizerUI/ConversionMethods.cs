using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIModelClasses;

namespace StocksVisualizerUI
{
    public static class ConversionMethods
    {
        public static List<UIStockInfo> To_UIStockInfo(List<Stocks_V> stocks)
        {
            var results = new List<UIStockInfo>();
            foreach (var stock in stocks)
            {
                var UIStock = new UIStockInfo();

                UIStock.ID = stock.StockID;
                UIStock.Symbol = stock.Symbol;
                UIStock.Company = stock.Company;
                UIStock.Exchange = stock.Exchange;
                UIStock.Sector = stock.Sector;
                UIStock.Industry = stock.Industry;
                UIStock.Description = string.IsNullOrEmpty(stock.Description) ? "No description available." : stock.Description;

                //daily data will get today too...if the day is closed 
                    //solution - last updated date daily shd be last OPEN date
                UIStock.MostRecentDate_Daily = (stock.MostRecent_DailyData.HasValue) ? stock.MostRecent_DailyData.Value.ToShortDateString() : "N/A";
                UIStock.LastUpdated_Minute = (stock.LastUpdated_MinData.HasValue) ? stock.LastUpdated_MinData.Value.ToShortDateString() : "N/A";
                UIStock.IsDisabled = (stock.IsDisabled == null) ? false : stock.IsDisabled.Value;
                UIStock.MarketCap = stock.MarketCap;
                UIStock.Last_Updated_MC = stock.Last_Updated_MC;
                UIStock.MostRecentDate_Stats = (stock.MostRecent_Stats.HasValue) ? stock.MostRecent_Stats.Value.ToShortDateString() : "N/A";

                results.Add(UIStock);

            }
            return results;
        }

        public static List<Stock> To_Stock(List<IEXStock> iexStocks, List<Sector> sectors)
        {
            List<Stock> stocks = new List<Stock>();
            foreach (var iexStock in iexStocks)
            {
                var stock = new Stock();
                var s = iexStock.company;
                if (String.IsNullOrEmpty(s.symbol) || String.IsNullOrEmpty(s.companyName))
                    continue;

                stock.Symbol = s.symbol;
                stock.Company = s.companyName;
                stock.Industry = s.industry;
                stock.Description = String.IsNullOrEmpty(s.description) ? null : s.description.ToString();
                stock.IssueType = s.issueType;
                stock.Employees = s.employees;
                stock.City = s.city;
                stock.State = s.state;
                stock.Country = s.country;
                stock.Exchange = s.exchange;

                var sector = String.IsNullOrEmpty(s.exchange) ? null : sectors.FirstOrDefault(x => x.Name.ToLower() == s.sector.ToLower());
                if (sector != null) { stock.SectorID = sector.ID; }
                stocks.Add(stock);

            }
            return stocks;

        }

        public static List<MarketCapitalization> To_MarketCapitalization(Dictionary<string, IEXStock_MC> dict, List<UIStockInfo> stocks)
        {
            List<MarketCapitalization> marketCaps = new List<MarketCapitalization>();
            foreach (var pair in dict)
            {
                var MC = new MarketCapitalization();

                var stock = stocks.FirstOrDefault(x => x.Symbol.ToString() == pair.Key);            
                if (stock == null) { continue; }

                MC.StockID = stock.ID;

                //0 indicates that the new market cap is null, DB update method will use the previous market cap but update the date
                //alternate solution: make market cap column nullable
                MC.MarketCap = (pair.Value.stats.marketCap == null) ? 0 : (long)pair.Value.stats.marketCap;
                MC.Date = DateTime.Today;
                marketCaps.Add(MC);

            }
            return marketCaps;
        }

        public static List<HistoricalData> From_Batch_To_HistData(Dictionary<string, IEXStock_HD> dict)
        {
            List<HistoricalData> results = new List<HistoricalData>();
            foreach (var pair in dict)
            {

                var charts = pair.Value.chart;
                foreach (var chart in charts)
                {
                    if ((chart.Volume == 0) || (String.IsNullOrEmpty(pair.Key)) || (chart.Date == null) || (chart.Close == 0)) { continue; }

                    var stock = MainWindow.UIstocks.FirstOrDefault(x => x.Symbol.ToUpper() == pair.Key.ToUpper());
                    if (stock == null) { continue; }

                    var temp = new HistoricalData
                    {
                        StockID = stock.ID,
                        Close = chart.Close,
                        Date = chart.Date,
                        Volume = chart.Volume,

                        Open = (chart.Open == 0) ? chart.Close : chart.Open,
                        High = (chart.High == 0) ? chart.Close : chart.High,
                        Low = (chart.Low == 0) ? chart.Close : chart.Low
                    };

                    results.Add(temp);
                }

            }

            return results;
        }

        public static List<HistoricalData> To_HistData(List<DailyDataModel> iex_data, Rootobject quandl_data, long stock_id)
        {
            var results = new List<HistoricalData>();
            var properties = quandl_data.datatable.data;
            foreach (var row in iex_data)
            {
                var result = new HistoricalData();

                result.StockID = stock_id;
                result.Close = row.Close;
                result.Open = row.Open;
                result.High = row.High;
                result.Low = row.Low;
                result.Volume = row.Volume;
                result.Date = row.Date;

                var item = properties.FirstOrDefault(x => x[1].ToString() == row.Date.ToString("yyyy-MM-dd"));
                if (item != null)
                { result.MarketCap = Convert.ToInt64(Math.Round((double)item[6], 0)); }

                results.Add(result);
            }
            return results;
        }

        public static List<MinuteData> To_MinuteData(List<MinuteDataModel> minuteDatas)
        {
            //cannot allow ANY NULL VALUES FOR ANY COLUMN
            var results = new List<MinuteData>();
            double? prev_average = null;
            foreach (var m in minuteDatas)
            {
                if(prev_average == null && m.marketAverage == null)
                {
                    continue;
                }

                var hour = Convert.ToInt32(m.minute.Remove(2));
                var minute = Convert.ToInt32(m.minute.Substring(3));
                var temp = new MinuteData
                {
                    AmtOfTrades = (m.marketNumberOfTrades != null) ? (long) m.marketNumberOfTrades : 0,
                    Volume = (m.marketVolume != null) ? (long)m.marketVolume : 0,
                    Date = m.date.AddHours(hour).AddMinutes(minute),
                    Average = (m.marketAverage != null) ? (double) m.marketAverage : (double) prev_average
                };
                results.Add(temp);
                if(m.marketAverage != null)
                {
                    prev_average = m.marketAverage;
                }
            }

            return results;
        }
        
    }
}

