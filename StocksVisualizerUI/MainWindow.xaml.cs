using Nager.Date;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using APIModelClasses;
using ExtensionMethods;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace StocksVisualizerUI
{
    public class API_SymbolFacade
    {
        public string Symbol { get; set; }
    }
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = LogHelper.GetLogger();

        public static List<Holiday> Holidays = new List<Holiday>();
        public static List<DateTime> fully_closed_holidays = new List<DateTime>();
        public static List<DateTime> partially_open_holidays = new List<DateTime>();

        public static List<UIStockInfo> UIstocks = new List<UIStockInfo>();
        public static List<UIStockInfo> Sorted_UIStocks = new List<UIStockInfo>();

        public static List<HistoricalData> histDatas = new List<HistoricalData>();

        int period = 20; // used when getting minute data for each stock -> .Take linq query 
        DateTime? last_updated_MC = new DateTime();
        //DateTime? latest_STATS = new DateTime();
        //int amt_stocks_displayed = 4000;

        int top_amt = 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            this.Loaded += new RoutedEventHandler(Window_loaded);
            ApiHelper.InitializeClient();
            //need to make data retrieval async so that page doesn't load for as long
        }

        private void Window_loaded(object sender, RoutedEventArgs e)
        {
            app_status.Content = "Loading data...";


            //get holidays
            using (var context = new NeeksDBEntities())
            {
                Holidays = context.Holidays.ToList();
            }
            fully_closed_holidays = Holidays.Where(x => !x.EarlyClose).Select(x => x.Date).ToList();
            partially_open_holidays = Holidays.Where(x => x.EarlyClose).Select(x => x.Date).ToList();

            //load grid
            RefreshGrid();
            app_status.Content = $"{UIstocks.Count} stocks loaded.";

        }
        
        private void RefreshGrid() //gets most recent data from DB & sets that as itemsSource for entire program & UI
        {
            List<Stocks_V> DBstocks = new List<Stocks_V>();

            using (var context = new NeeksDBEntities())
            {
                DBstocks = context.Stocks_V.OrderByDescending(x => x.MarketCap).ToList();
            }

            if (!DBstocks.Any())
                return;
            UIstocks = ConversionMethods.To_UIStockInfo(DBstocks);

            last_updated_MC = UIstocks.Min(x => x.Last_Updated_MC);
            //latest_STATS = UIstocks.Min(x => x.Last_Updated_Stats);

            stock_info_datagrid.ItemsSource = UIstocks;

            //update_stats.ToolTip = (latest_STATS == null) ? "Latest Stats: N/A" : $"Last Updated: {latest_STATS.Value.ToLongDateString()}";
            update_mc_btn.ToolTip = (last_updated_MC == null) ? "Last Updated: N/A" : $"Last Updated: {last_updated_MC.Value.ToLongDateString()}";
        }

        private void Period_txtbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                var input = period_txtbox.Text;
                if (string.IsNullOrEmpty(input))
                {
                    period_txtbox.Text = "20";
                    period = 20;
                    return;
                }
                bool is_successful = int.TryParse(input, out period);
                if (!is_successful)
                {
                    MessageBox.Show("Please type a number.");
                }
            }

        }

        private async void Update_mc_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                update_mc_btn.IsEnabled = false;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //check to make sure the daterange exists at all & makes sure it doesn't consist of solely weekends and holidays 
                var daterange = BusinessDateRange.DatesBetween(last_updated_MC.Value, DateTime.Today, fully_closed_holidays);

                if ((last_updated_MC == DateTime.Today) || (!daterange.Any()))
                {
                    MessageBox.Show("The market caps stored are already up to date.");
                }

                List<string> symbols = new List<string>(UIstocks.Where(x => !x.IsDisabled).
                    Where(x => x.Last_Updated_MC != DateTime.Today).Select(x => x.Symbol));
                var every_100_symbols_list = SplitSymbolsList(symbols);
                progress_bar.Maximum = symbols.Count;
                int total = 0;


                await Task.Run(() =>
                {
                    var temp = new List<MarketCapitalization>();
                    foreach (var every_100_symbols in every_100_symbols_list)
                    {
                        string mc_url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/stock/market/batch?symbols={string.Join(",", every_100_symbols)}&types=stats&token={ConfigurationManager.AppSettings["iex_token"]}";

                        var iex_marketcaps = ApiDataProcessor.Get_API_Obj_Dict<string, IEXStock_MC>(mc_url);
                        var marketCaps = ConversionMethods.To_MarketCapitalization(iex_marketcaps.Result, UIstocks);
                        temp.AddRange(marketCaps);
                        DBchanges.Update_MarketCaps(marketCaps);
                        total += every_100_symbols.Count;
                        Edit_Status_Label($"Updated market caps for {total} stocks. Time Elapsed: {stopwatch.ElapsedMilliseconds / 1000} s ");
                        Increment_Progress_Bar(every_100_symbols.Count);
                    }
                });

                app_status.Content = $"Successfully updated market caps for all {total} stocks in database. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s";

                RefreshGrid();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        log.Error(message);
                    }
                }
                MessageBox.Show("ERROR - DbEntityValidationException occured. See log for more details.");
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException != null ? $"CURRENT EXCEPTION - {ex.ToString()} \n\nINNER EXCEPTION - {ex.InnerException.ToString()}" : ex.ToString());
                MessageBox.Show($"ERROR - {ex.Message}");
            }
            finally
            {
                update_mc_btn.IsEnabled = true;
                progress_bar.Value = 0;
            }
            //will need to refreshgrid - will take care of last updates date + datagrid items source refresh
        }

        private void Search_stocks_TextChanged(object sender, TextChangedEventArgs e)
        {
            var choice = search_stocks.Text;
            stock_info_datagrid.ItemsSource = (string.IsNullOrEmpty(top_stocks_amt.Text)) ? UIstocks.Where(x => x.Symbol.StartsWith(choice.ToUpper())) : Sorted_UIStocks.Where(x => x.Symbol.StartsWith(choice.ToUpper()));
        }

        private void Top_stocks_amt_HitEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //testing can be done here
                    //int count = UIstocks.Where(x => x.Last_Updated_MC.Value < DateTime.Today.AddDays(-1)).Count();

                if (UIstocks.All(x=> x.MarketCap == null))
                {
                    MessageBox.Show("Unable to filter by market cap because the column is empty. Update market caps to solve issue.");
                    return;
                }
                var input = top_stocks_amt.Text;
                if (string.IsNullOrEmpty(input))
                {
                    stock_info_datagrid.ItemsSource = UIstocks;
                    top_amt = 0;
                    return;
                }
                bool is_successful = int.TryParse(input, out top_amt);
                if (!is_successful)
                {
                    MessageBox.Show("Please type a number.");
                }
                else
                {
                    Sorted_UIStocks = UIstocks.OrderByDescending(x => x.MarketCap).Take(top_amt).ToList();
                    stock_info_datagrid.ItemsSource = Sorted_UIStocks;
                }

            }
        }

        private void Select_Or_Deselect_All(object sender, RoutedEventArgs e)
        {
            if(top_amt == 0)
            {
                if (!UIstocks.All(x => x.IsStockChecked == true))
                {
                    foreach (UIStockInfo item in UIstocks)
                    {
                        item.IsStockChecked = true;
                    }
                }
                else if (!UIstocks.All(x => x.IsStockChecked == false))
                {
                    foreach (UIStockInfo item in  UIstocks)
                    {
                        item.IsStockChecked = false;
                    }

                }
                stock_info_datagrid.ItemsSource = null;
                stock_info_datagrid.ItemsSource = UIstocks;

            }
            else
            {
                if (!UIstocks.Take(top_amt).All(x => x.IsStockChecked == true))
                {
                    foreach (UIStockInfo item in UIstocks.Take(top_amt))
                    {
                        item.IsStockChecked = true;
                    }
                }
                else if (!UIstocks.Take(top_amt).All(x => x.IsStockChecked == false))
                {
                    foreach (UIStockInfo item in UIstocks.Take(top_amt))
                    {
                        item.IsStockChecked = false;
                    }

                }
                stock_info_datagrid.ItemsSource = null;
                stock_info_datagrid.ItemsSource = UIstocks.Take(top_amt);
            }

        }
    
        private async void Update_go_btn_Click(object sender, RoutedEventArgs e) //see how to shorten this (maybe shift some over to methods)
        {
            try
            {
                update_btn.IsEnabled = false;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                var selected_stocks = new List<UIStockInfo>(UIstocks.Where(x => !x.IsDisabled).Where(x => x.IsStockChecked == true));

                if (!selected_stocks.Any())
                {
                    SoundPlayer soundPlayer = new SoundPlayer();
                    soundPlayer.PlaySync();
                    MessageBox.Show("Please select one or more stocks.");
                    return;
                }

                stocksWithHistData.IsChecked = false;

                if (update_daily_data.IsSelected)
                {
                    #region Update Daily Data

                    //var market_close = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 14, 30, 0, DateTimeKind.Local);  //2:30 PM bc that's 3:30 in EST
                
                    if (selected_stocks.All(x => (x.MostRecentDate_Daily == DateTime.Today.ToShortDateString()) || (x.MostRecentDate_Daily == DateTime.Today.AddDays(-1).ToShortDateString())))
                    {
                        SoundPlayer soundPlayer = new SoundPlayer();
                        soundPlayer.PlaySync();
                        app_status.Content = "The stocks selected already have the most recent daily data stored in the database.";
                        return;
                    }


                    progress_bar.Maximum = selected_stocks.Count;

                    string iex_url;
                    string quandl_url;
                    int count = 0;

                    var selected_stocks_split = SplitStocksList(selected_stocks);

                    ParallelOptions parallelOptions = new ParallelOptions();
                    parallelOptions.MaxDegreeOfParallelism = 10;


                    await Task.Run(() =>
                    {
                        #region parallel foreach
                        foreach (var selectedStocks in selected_stocks_split)
                        {
                            Parallel.ForEach(selectedStocks, parallelOptions, stock =>
                            {
                                if (stock.MostRecentDate_Daily == "N/A")
                                {
                                    //iex
                                    iex_url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/stock/{stock.Symbol}/chart/1y/?token={ConfigurationManager.AppSettings["iex_token"]}";
                                    var iex_data = ApiDataProcessor.Get_API_Obj_List<DailyDataModel>(iex_url).Result;

                                    //quandl
                                    quandl_url = $"{ConfigurationManager.AppSettings["quandl_baseURL"]}?api_key={ConfigurationManager.AppSettings["quandl_api_key"]}&ticker={stock.Symbol}&" +
                                            $"date.gte={DateTime.Today.AddDays(-1).AddYears(-1).ToString("yyyy-MM-dd")}&date.lte={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}";
                                    var quandl_data = ApiDataProcessor.Get_API_Obj<Rootobject>(quandl_url);

                                    var db_data = ConversionMethods.To_HistData(iex_data, quandl_data.Result, stock.ID);
                                    DBchanges.Insert_HistDatas(db_data);
                                    count += db_data.Count;
                                    Edit_Status_Label($"Stored {db_data.Count} rows in database for {stock.Symbol}. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s.");
                                }
                                else
                                {
                                    DateTime most_recent_date = DateTime.Parse(stock.MostRecentDate_Daily);
                                    if(most_recent_date == DateTime.Today)
                                    {
                                        Increment_Progress_Bar();
                                        return; //continue; 
                                    }

                                    List<DateTime> dates = BusinessDateRange.DatesBetween(most_recent_date.AddDays(1), DateTime.Today, fully_closed_holidays);

                                    if (!dates.Any())
                                    {
                                        Increment_Progress_Bar();
                                        return; //continue; 
                                    }

                                    var iex_data = new List<DailyDataModel>();
                                    foreach (var date in dates)
                                    {
                                        iex_url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/stock/{stock.Symbol}/chart/date/{string.Format("{0:yyyyMMdd}", date)}?chartByDay=true&token={ConfigurationManager.AppSettings["iex_token"]}";
                                        iex_data.AddRange(ApiDataProcessor.Get_API_Obj_List<DailyDataModel>(iex_url).Result);
                                    }


                                    var min_date = DateTime.Parse(stock.MostRecentDate_Daily);
                                    quandl_url = $"{ConfigurationManager.AppSettings["quandl_baseURL"]}?api_key={ConfigurationManager.AppSettings["quandl_api_key"]}&ticker={stock.Symbol}&" +
                                        $"date.gte={min_date.ToString("yyyy-MM-dd")}&date.lte={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}";
                                    var quandl_data = ApiDataProcessor.Get_API_Obj<Rootobject>(quandl_url);

                                    var db_data = ConversionMethods.To_HistData(iex_data, quandl_data.Result, stock.ID);
                                    DBchanges.Insert_HistDatas(db_data);
                                    count += db_data.Count;
                                    Edit_Status_Label($"Stored {db_data.Count} rows in database for {stock.Symbol} from {dates.Min().ToShortDateString()} to {dates.Max().ToShortDateString()}. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s.");
                                }
                                Increment_Progress_Bar();
                            });
                        }
                        #endregion
                        #region non-parallel foreach 
                        //foreach (var stock in selected_stocks)  
                        //{
                        //    //get one year's worth of data 
                        //    if (stock.LastUpdatedDate_Daily == "N/A")
                        //    {
                        //        //iex
                        //        iex_url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/stock/{stock.Symbol}/chart/1y/?token={ConfigurationManager.AppSettings["iex_token"]}";
                        //        var iex_data = ApiDataProcessor.Get_API_Obj_List<DailyDataModel>(iex_url).Result;

                        //        //quandl
                        //        quandl_url = $"{ConfigurationManager.AppSettings["quandl_baseURL"]}?api_key={ConfigurationManager.AppSettings["quandl_api_key"]}&ticker={stock.Symbol}&" +
                        //            $"date.gte={DateTime.Today.AddDays(-1).AddYears(-1).ToString("yyyy-MM-dd")}&date.lte={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}";
                        //        var quandl_data = ApiDataProcessor.Get_API_Obj<Rootobject>(quandl_url);

                        //        var db_data = ConversionMethods.To_HistData(iex_data, quandl_data.Result, stock.ID);
                        //        DBchanges.Insert_HistDatas(db_data);
                        //        count += db_data.Count;
                        //        Edit_Status_Label($"Stored {db_data.Count} rows in database for {stock.Symbol}. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s.");
                        //    }
                        //    else
                        //    {
                        //        DateTime last_updated_date = DateTime.Parse(stock.LastUpdatedDate_Daily); 
                        //        List<DateTime> dates = BusinessDateRange.DatesBetween(last_updated_date, DateTime.Today, fully_closed_holidays);

                        //        if (!dates.Any())
                        //        {
                        //            Increment_Progress_Bar();
                        //            continue;
                        //        }

                        //        var iex_data = new List<DailyDataModel>();
                        //        foreach (var date in dates)
                        //        {
                        //            iex_url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/stock/{stock.Symbol}/chart/date/{string.Format("{0:yyyyMMdd}", date)}?chartByDay=true&token={ConfigurationManager.AppSettings["iex_token"]}";
                        //            iex_data.AddRange(ApiDataProcessor.Get_API_Obj_List<DailyDataModel>(iex_url).Result);
                        //        }


                        //        var min_date = DateTime.Parse(stock.LastUpdatedDate_Daily);
                        //        quandl_url = $"{ConfigurationManager.AppSettings["quandl_baseURL"]}?api_key={ConfigurationManager.AppSettings["quandl_api_key"]}&ticker={stock.Symbol}&" +
                        //            $"date.gte={min_date.ToString("yyyy-MM-dd")}&date.lte={DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")}";
                        //        var quandl_data = ApiDataProcessor.Get_API_Obj<Rootobject>(quandl_url);

                        //        var db_data = ConversionMethods.To_HistData(iex_data, quandl_data.Result, stock.ID);
                        //        DBchanges.Insert_HistDatas(db_data);
                        //        count += db_data.Count;
                        //        Edit_Status_Label($"Stored {db_data.Count} rows in database for {stock.Symbol} from {dates.Min().ToShortDateString()} to {dates.Max().ToShortDateString()}. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s.");

                        //    }
                        //    Increment_Progress_Bar();
                        //}
                        #endregion
                    });
                    app_status.Content = $"Successfully stored {count} total rows in database for {selected_stocks.Count} stocks. Total time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s";

                    #endregion

                    RefreshGrid();
                }
                else if (update_minute_data.IsSelected)
                {
                    if (selected_stocks.All(x => x.LastUpdated_Minute == DateTime.Today.ToShortDateString()))
                    {
                        SoundPlayer soundPlayer = new SoundPlayer();
                        soundPlayer.PlaySync();
                        app_status.Content = "The stocks selected already have the most recent minute data stored in the database.";
                        return;
                    }

                    progress_bar.Maximum = selected_stocks.Count;

                    int total_count = 0;
                    await Task.Run(() =>
                    {
                        string url;                        
                        foreach (var stock in selected_stocks)
                        {
                            int count = 0;
                            List<DateTime> dates = new List<DateTime>();
                            //choosing date range - right now we're not accounting for the 3:30 iex updates, simply using the day previous from today
                            if (stock.LastUpdated_Minute == "N/A")
                            {
                                dates = BusinessDateRange.DatesBefore(DateTime.Today, 20, fully_closed_holidays);
                            }
                            else
                            {
                                //get days since today and last updated date in db 
                                DateTime last_updated_date = DateTime.Parse(stock.LastUpdated_Minute);
                                dates = BusinessDateRange.DatesBetween(last_updated_date, DateTime.Today, fully_closed_holidays);

                                if (!dates.Any())
                                {
                                    Increment_Progress_Bar();
                                    continue;
                                }
                            }

                            int? lastDay_nullRows = null;
                            bool? lastDay_isEarlyClose = null;

                            foreach (var date in dates)
                            {
                                log.Info(string.Format("Getting date for Symbol: {0}, date : {1}", stock.Symbol, date));

                                url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/stock/{stock.Symbol}/chart/1d?exactDate={string.Format("{0:yyyyMMdd}", date)}&token={ConfigurationManager.AppSettings["iex_token"]}";
                                var API_minute_data = ApiDataProcessor.Get_API_Obj_List<MinuteDataModel>(url);

                                var db_data = ConversionMethods.To_MinuteData(API_minute_data.Result);
                                foreach (var row in db_data)
                                {
                                    row.StockID = stock.ID;
                                }

                                DBchanges.Insert_MinuteDatas(db_data);

                                //supply values for PercentData_LastDay column
                                if (date == dates.Max())
                                {
                                    lastDay_nullRows = db_data.Where(x => x.Volume == 0).Count();
                                    lastDay_isEarlyClose = partially_open_holidays.Contains(date);
                                }

                                total_count += db_data.Count;
                                count += db_data.Count;
                                Edit_Status_Label($"Stored {count} rows in database for {stock.Symbol}. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s.");

                            }
                            //update PercentData_LastDay & PercentData_SamplePeriod columns 
                            decimal lastDay_val = ((bool)lastDay_isEarlyClose ? ((240m - (decimal)lastDay_nullRows) / 240m) : ((390m - (decimal)lastDay_nullRows) / 390m)) * 100;
                            decimal samplePeriod_val;
                            using (var context = new NeeksDBEntities())
                            {

                                var daterange = new List<DateTime>(context.GetDateRange_Period(period, stock.ID).Select(x => x.Value));
                                var from = daterange.Min();
                                var to = daterange.Max().AddDays(1);
                                var samplePeriod_nullRows = context.MinuteDatas.Where(x => x.StockID == stock.ID).Where(x => x.Date >= from && x.Date < to).Where(x => x.Volume == 0).Count();

                                var daysEarlyClose = (partially_open_holidays.Intersect(daterange)).Count();

                                var total = (240m * daysEarlyClose) + (390m * (period - daysEarlyClose));
                                samplePeriod_val = ((total - samplePeriod_nullRows) / total) * 100;  
                            }
                            DBchanges.Update_MinDataColumns_In_Stock(stock.ID, lastDay_val, samplePeriod_val);

                            Increment_Progress_Bar();
                        }
                    }); //error got thrown right here...
                    app_status.Content = $"Successfully stored {total_count} total rows of minute data in database for {selected_stocks.Count} stocks. Total time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s";
                    RefreshGrid();
                }
                else if (update_stats.IsSelected)
                {
                    await UpdateStats(selected_stocks);
                }

             }
            #region catch + finally block
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        log.Error(message);
                    }
                }
                MessageBox.Show("ERROR - DbEntityValidationException occured. See log for more details.");
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException != null ? $"CURRENT EXCEPTION - {ex.ToString()} \n\nINNER EXCEPTION - {ex.InnerException.ToString()}" : ex.ToString());
                MessageBox.Show($"ERROR - {ex.Message}");
            }
            finally
            {
                update_btn.IsEnabled = true;
                progress_bar.Value = 0;
            }
            #endregion
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = (DataGridRow) e.Source;
            var dc = (UIStockInfo) row.DataContext;
                       
            if (UIstocks.FirstOrDefault(x=> x.ID == dc.ID).MostRecentDate_Daily != "N/A")
            {
                ModelessChartPopUp chartPopUp = new ModelessChartPopUp(dc);
                chartPopUp.Show();
                // show new popup window based on the selected row
            }
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboard = new DashboardWindow();
            dashboard.ShowDialog();
        }

        private async Task UpdateStats(List<UIStockInfo> selectedStocks)
        {
            try
            {
                update_stats.IsEnabled = false;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //get stocks to calculate stats for
                var stocks_to_calculate = selectedStocks.Where(x => x.LastUpdated_Minute != "N/A").Select(x => new UpdateStatsFacade { ID = x.ID, Symbol = x.Symbol,
                    MostRecent_Stats = (x.MostRecentDate_Stats == "N/A") ? null : (DateTime?)DateTime.Parse(x.MostRecentDate_Stats),
                }).ToList();
                //find a simpler less costly way to do this?

                if (!stocks_to_calculate.Any())
                {
                    MessageBox.Show("There is no minute data available for the selected stocks to calculate stats on.");
                    return;
                }
                if (stocks_to_calculate.All(x=> x.MostRecent_Stats == DateTime.Today) )
                {
                    MessageBox.Show("Calculations are already up to date.");
                    return;
                }
                            
                await Task.Run(() =>
                {
                    Set_Progress_Bar_Max(stocks_to_calculate.Count);

                    //can we make this parallel?
                    Parallel.ForEach(stocks_to_calculate, stock =>
                    {
                        Edit_Status_Label($"Calculating and storing stats for {stock.Symbol}. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s");
                        using (var context = new NeeksDBEntities())
                        {
                            stock.MostRecent_MinData = context.MinuteDatas.Where(x => x.StockID == stock.ID).Max(x => x.Date).Date;
                        }

                        if (stock.MostRecent_Stats == stock.MostRecent_MinData)
                        {
                            Increment_Progress_Bar();
                            return;
                        }

                        DateTime? start = (stock.MostRecent_Stats.HasValue) ? stock.MostRecent_Stats.Value.AddDays(1) : stock.MostRecent_Stats;
                        DateTime? end = (start.HasValue) ? (DateTime?)stock.MostRecent_MinData : null;

                        var minute_datas = new List<MinuteData>(); //feel like this way might be slower ...see if its faster to make db call in next foreach (after done writing method)
                        var daterange = new List<DateTime?>();

                        using (var context = new NeeksDBEntities())
                        {
                            //gets all dates between start and end INCLUDING start and end 
                            daterange = context.GetDateRange_StartEnd(start, end, stock.ID).ToList();
                            DateTime? stop = (end.HasValue) ? (DateTime?)end.Value.AddDays(1) : null;
                            minute_datas = (start.HasValue)
                                ? context.MinuteDatas.Where(x => x.StockID == stock.ID).Where(x => x.Date >= start.Value && x.Date < stop).ToList()
                                : context.MinuteDatas.Where(x => x.StockID == stock.ID).ToList();
                        }

                        if (minute_datas.Any(x => x.Average == 0)) //probs uneccesary now that u have disabled column in stocks table 
                        {
                            Increment_Progress_Bar();
                            return;
                        }
                        var sample_stats = new List<SampleStat>();
                        //if u wanted to make this parallel too then you would have to make sample stats a Concurrent Bag instead of a list
                        foreach (var day in daterange) //get Total # of Trades + Standard Deviation
                        {
                            var from = (DateTime)day;
                            var to = ((DateTime)day).AddDays(1);
                            var todays_data = minute_datas.Where(x => x.Date >= from && x.Date < to);

                            var st_dev = todays_data.Select(x => x.Average).StandardDev();
                            var total_trades = todays_data.Select(x => x.AmtOfTrades).Sum();
                            sample_stats.Add(new SampleStat { StockID = stock.ID, Date = day.Value, StDev = Math.Round(st_dev, 5), Total_AmtOfTrades = total_trades });
                        }
                        //insert all values for that stock 
                        DBchanges.Insert_Stats(sample_stats);
                        Increment_Progress_Bar();
                    });
                });

                //UI updates  
                app_status.Content = $"Successfully updated stats for {stocks_to_calculate.Count} stocks in database. Time elapsed: {stopwatch.ElapsedMilliseconds / 1000} s ";
                RefreshGrid(); //is it necessary to refresh the entire grid again?
            }
            #region catch + finally blocks
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        string message = string.Format("{0}:{1}",
                            validationErrors.Entry.Entity.ToString(),
                            validationError.ErrorMessage);
                        log.Error(message);
                    }
                }
                MessageBox.Show("ERROR - DbEntityValidationException occured. See log for more details.");
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException != null ? $"CURRENT EXCEPTION - {ex.ToString()} \n\nINNER EXCEPTION - {ex.InnerException.ToString()}" : ex.ToString());
                MessageBox.Show($"ERROR - {ex.Message}");
            }
            finally
            {
                update_stats.IsEnabled = true;
                progress_bar.Value = 0;
            }
            #endregion
        }

        //Dispatcher.Invoke() methods
        public void Increment_Progress_Bar(int val = 1)
        {
            if(val == 0)
            {
                MessageBox.Show("Cannot increment progress bar by 0.");
                return;
            }
            Dispatcher.Invoke(() => (val == 1) ? progress_bar.Value++ : progress_bar.Value+= val);
        }

        public void Set_Progress_Bar_Max(int value)
        {
            Dispatcher.Invoke(() => progress_bar.Maximum = value);
        }

        public void Edit_Status_Label(string msg)
        {
            Dispatcher.Invoke(() => app_status.Content = msg);
        }

        // methods that are NOT event handlers
        public async Task<List<List<string>>> PrepareSymbolsForBatchRequest()
        {

            //  ApiHelper.InitializeClient();
            string symbols_url = $"{ConfigurationManager.AppSettings["iex_baseURL"]}stable/ref-data/symbols?token={ConfigurationManager.AppSettings["iex_token"]}";
            List<API_SymbolFacade> symbols = await ApiDataProcessor.Get_API_Obj_List<API_SymbolFacade>(symbols_url);

            List<string> string_symbols = new List<string>();
            foreach (var obj in symbols)
            {
                string_symbols.Add(obj.Symbol);
            }

            var every_100_symbols_list = SplitSymbolsList(string_symbols);

            return every_100_symbols_list;
        }

        public static List<List<string>> SplitSymbolsList(List<string> symbols, int nSize = 100)
        {

            List<List<string>> results = new List<List<string>>();
            for (int i = 0; i < symbols.Count; i += nSize)
            {
                results.Add(symbols.GetRange(i, Math.Min(nSize, symbols.Count - i)));

            }

            return results;
        }

        public static List<List<UIStockInfo>> SplitStocksList(List<UIStockInfo> symbols, int nSize = 10)
        {

            List<List<UIStockInfo>> results = new List<List<UIStockInfo>>();
            for (int i = 0; i < symbols.Count; i += nSize)
            {
                results.Add(symbols.GetRange(i, Math.Min(nSize, symbols.Count - i)));
            }

            return results;
        }

        int i = 0;
        private void Disabled_checkbox_Changed(object sender, RoutedEventArgs e)
        {
            if (i == 0)
            {
                i++;
                return;
            }

            var checkbox = (CheckBox)e.Source;
            var val = checkbox.IsChecked;
            var id = ((UIStockInfo)checkbox.DataContext).ID;
            Task.Run(() =>
            {
                DBchanges.Update_IsDisabledColumn_In_Stock(id, val.Value);
            });
            //RefreshGrid();
        }

        private void StocksWithHistData_Checked(object sender, RoutedEventArgs e)
        {
            if(top_amt == 0)
            {
                foreach (var item in UIstocks.Where(x => x.MostRecentDate_Daily != "N/A"))
                {
                    item.IsStockChecked = true;
                }
            }
            else
            {
                foreach (var item in UIstocks.Take(top_amt).Where(x => x.MostRecentDate_Daily != "N/A"))
                {
                    item.IsStockChecked = true;
                }

            }
            stock_info_datagrid.ItemsSource = null;
            stock_info_datagrid.ItemsSource = UIstocks;
            app_status.Content = $"{UIstocks.Count(x => x.IsStockChecked)} stocks selected.";
        }

        private void StocksWithHistData_Unchecked(object sender, RoutedEventArgs e)
        {
            if (top_amt == 0)
            {
                foreach (var item in UIstocks.Where(x => x.MostRecentDate_Daily != "N/A"))
                {
                    item.IsStockChecked = false;
                }
            }
            else
            {
                foreach (var item in UIstocks.Take(top_amt).Where(x => x.MostRecentDate_Daily != "N/A"))
                {
                    item.IsStockChecked = false;
                }

            }
            stock_info_datagrid.ItemsSource = null;
            stock_info_datagrid.ItemsSource = UIstocks;
            //app_status.Content = $"{UIstocks.Count(x => x.IsStockChecked)} stocks selected.";

        }
    }

    public class UpdateStatsFacade
    {
        public long ID { get; set; }
        public string Symbol { get; set; }
        public DateTime  MostRecent_MinData { get; set; }
        public DateTime? MostRecent_Stats { get; set; }
    }
}
