using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Helpers;
using LiveCharts.Wpf;
using Nager.Date;
using Utilities;

namespace StocksVisualizerUI
{
    /// <summary>
    /// Interaction logic for DashboardWindow.xaml
    /// </summary>
    /// 
    //Facade Classes
    public class SectorData_VFacade
    {
        public double Value { get; set; }
        public string Sector { get; set; }
        public int SectorId { get; set; }
    }
    public class SymbolFacade
    {
        public long ID { get; set; }
        public string Symbol { get; set; }
        public string Company { get; set; }
        public bool IsChartOpen { get; set; }
    }
    public class HistDataFacade
    {
        public long StockID { get; set; }
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public long Volume { get; set; }
    }
    public class ScannerFacade
    {
        public long StockID { get; set; }
        public DateTime Date { get; set; }
        public double Close { get; set; }
    }
    public class Scanner_DatagridsFacade
    {
        public long StockID { get; set; }
        public string Symbol { get; set; }
        public string Company { get; set; }
        public TrendDirection Trend { get; set; }
    }

    public enum TrendDirection
    {
        None, Uptrend, Downtrend, Consolidating
    }

    public partial class DashboardWindow : Window
    {
        #region global variables
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        List<SymbolFacade> symbols = new List<SymbolFacade>();
        public static List<Holiday> holidays = new List<Holiday>();

        //sector allocation variables
        List<SectorData_VFacade> piechart_data = new List<SectorData_VFacade>();
        public Func<ChartPoint, string> PointLabel { get; set; }

        //OHLC variables
        List<HistDataFacade> ohlcv_chart_data = new List<HistDataFacade>();
        //public DateTime[] ohlcv_Labels { get; set; }
        public Func<double, string> Volume_YFormatter { get; set; }
        public Func<double, string> OHLC_YFormatter { get; set; }
        private DateTime _initialDateTime;
        private PeriodUnits _period = PeriodUnits.Days;
        public event PropertyChangedEventHandler PropertyChanged;
        private string[] _ohlc_labels;

        //trend scanner variables 
        //slope configuration (vals in degrees):
        public double consolidation_min_degrees = -10;
        public double consolidation_max_degrees = 10;
        public bool IsDegrees = true;

            //global lists for the scanner columns 
        //public List<Scanner_DatagridsFacade> scanner_1yr = new List<Scanner_DatagridsFacade>();
        //public List<Scanner_DatagridsFacade> scanner_6mo = new List<Scanner_DatagridsFacade>();
        //public List<Scanner_DatagridsFacade> scanner_3mo = new List<Scanner_DatagridsFacade>();
        //public List<Scanner_DatagridsFacade> scanner_1mo = new List<Scanner_DatagridsFacade>();

        public ConcurrentBag<Scanner_DatagridsFacade> scanner_1yr = new ConcurrentBag<Scanner_DatagridsFacade>();
        public ConcurrentBag<Scanner_DatagridsFacade> scanner_6mo = new ConcurrentBag<Scanner_DatagridsFacade>();
        public ConcurrentBag<Scanner_DatagridsFacade> scanner_3mo = new ConcurrentBag<Scanner_DatagridsFacade>();
        public ConcurrentBag<Scanner_DatagridsFacade> scanner_1mo = new ConcurrentBag<Scanner_DatagridsFacade>();



        #endregion

        public DashboardWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Window_loaded);
        }

        private void Window_loaded(object sender, RoutedEventArgs e)
        {
            Period = PeriodUnits.Days; 
            using (var context = new NeeksDBEntities())
            {
                //don't include disabled stocks - isDisabled would be either null or
                symbols = context.Stocks_V.Where(x => (x.MostRecent_DailyData != null)).Where(x=> (!x.IsDisabled.HasValue) || (!x.IsDisabled.Value)).Select(x =>
                new SymbolFacade { ID = x.StockID, Symbol = x.Symbol, Company = x.Company, IsChartOpen = false }).ToList();
                //set the isChart Open = false here
                holidays = context.Holidays.ToList();
            }
            stocks_selector.ItemsSource = symbols.OrderBy(x=> x.Symbol);
            //stocks_selector.SelectedIndex = 0;
            sa_choice.IsSelected = true;

        }

        public DateTime InitialDateTime
        {
            get { return _initialDateTime; }
            set
            {
                _initialDateTime = value;
                OnPropertyChanged("InitialDateTime");
            }
        }

        public PeriodUnits Period
        {
            get { return _period; }
            set
            {
                _period = value;
                OnPropertyChanged("Period");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public enum ChartOptions
        //{
        //    OHLC_with_Volume, Sector_Allocation
        //}

        //private void SetDatePickerRanges(ChartOptions chart_chosen) //update this as the enum options grow
        //{
        //}

        private bool IsDaterangeValid(DateTime start_date, DateTime end_date) //DEBUG HOLIDAYS CODE
        {
            // make sure that end date isn't before start date
            if (start_date > end_date)
            {
                MessageBox.Show("End date cannot be before the start date.");
                return false;
            }       
            //check if daterange only includes weekends and/or holidays
            DateTime dateTime = start_date;
            var daterange = new List<DateTime>();
            var closed_holidays = holidays.Where(x => !x.EarlyClose).Select(x => x.Date);
            while (dateTime <= end_date) // initially added a day to end date, not sure why?
            {
                if (!DateSystem.IsWeekend(dateTime, CountryCode.US) && !closed_holidays.Contains(dateTime)) 
                {
                    daterange.Add(dateTime);
                }
                dateTime = dateTime.AddDays(1);
            }
            if (!daterange.Any())
            {
                MessageBox.Show("Date range cannot consist of only weekends and/or holidays.");
                return false;
            }
            return true;
        }

        private void RefreshPieChart() //refreshes pie chart according to new dates
        {
            start_date.DisplayDateStart = new DateTime(2020, 05, 04);
            start_date.DisplayDateEnd = new DateTime(2020, 11, 25);
            end_date.DisplayDateStart = new DateTime(2020, 05, 04);
            end_date.DisplayDateEnd = new DateTime(2020, 11, 25);

            if (!IsDaterangeValid(start_date.SelectedDate.Value, end_date.SelectedDate.Value))
            {
                return;
            }

            //get data from db
            using (var context = new NeeksDBEntities())
            {
                piechart_data = context.GetValuesGroupedBySector_sp(start_date.SelectedDate.Value, end_date.SelectedDate.Value.AddDays(1)).
                    Select(x => new SectorData_VFacade { Sector = x.Sector, SectorId = x.SectorId, Value = (double)x.Value }).ToList();
            }

            //convert db data to a range that scales properly to pie chart
            var oldmin = piechart_data.Min(x => x.Value);
            var oldmax = piechart_data.Max(x => x.Value);
            var UI_piechart_dict = new Dictionary<string, double>();
            foreach (var row in piechart_data)
            {
                var new_val = Normalizations.ConvertRange(row.Value, oldmin, oldmax, 0, 100);
                UI_piechart_dict.Add(row.Sector, new_val);
            }
        
            //chart code
            SeriesCollection piechart_sc = new SeriesCollection();
            foreach (var item in UI_piechart_dict)
            {
                piechart_sc.Add(new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<double> { item.Value },
                    DataLabels = true,
                    LabelPoint = PointLabel = chartPoint => string.Format("{0:P}", chartPoint.Participation)
                });
            }
            sa_chart_title.Content = $"Sector Allocation: {start_date.SelectedDate.Value.ToShortDateString()} to {end_date.SelectedDate.Value.ToShortDateString()}";
            sa_piechart.Series = piechart_sc;
        }
   
        public void RefreshOHLCV_Chart()
        {
            if (!IsDaterangeValid(start_date.SelectedDate.Value, end_date.SelectedDate.Value))
            {
                return;
            }
            if (stocks_selector.SelectedItem == null)
            {
                MessageBox.Show("Please select a stock.");
                return;
            }
            var stock = ((SymbolFacade)stocks_selector.SelectedItem);

            //check if stock id has changed (limits unecessary database queries)
            var first = ohlcv_chart_data.FirstOrDefault();
            if ( first == null ||(first.StockID != stock.ID))
            {
                using (var context = new NeeksDBEntities())
                {
                    ohlcv_chart_data = context.HistoricalDatas.Where(x => (x.StockID == stock.ID))
                        .Select(x => new HistDataFacade
                        {
                            Close = x.Close,
                            High = x.High,
                            Low = x.Low,
                            Open = x.Open,
                            Volume = x.Volume,
                            Date = x.Date,
                            StockID = x.StockID

                        }).ToList();
                }
            }
            DateTime from = ohlcv_chart_data.Min(x => x.Date);
            DateTime to = ohlcv_chart_data.Max(x => x.Date);

            start_date.DisplayDateStart = from;
            start_date.DisplayDateEnd = to;
            end_date.DisplayDateStart = from;
            end_date.DisplayDateEnd = to;

            //chart data binding (use only values for selected dates) 

            SeriesCollection ohlcv_sc = new SeriesCollection();

            //ohlc series
            var ohlc_series = new OhlcSeries
            {
                Title = "Prices",
                Values = new ChartValues<OhlcPoint>(ohlcv_chart_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate)
                .Select(x => new OhlcPoint
                {
                    Open = x.Open,
                    High = x.High,
                    Low = x.Low,
                    Close = x.Close
                }))
            };
            OHLC_YFormatter = value => value.ToString("C");
            ohlc_y_axis.LabelFormatter = OHLC_YFormatter;
            ohlcv_sc.Add(ohlc_series);

            //volume line series
            //var lineSeries = new LineSeries
            //{
            //    Title = "Volume",
            //    Fill = new SolidColorBrush
            //    {
            //        Color = Color.FromRgb(132, 132, 220),
            //        Opacity = .4
            //    },
            //    Stroke = new SolidColorBrush
            //    {
            //        Color = Color.FromRgb(132, 132, 220)
            //    },
            //    Values = new ChartValues<double>(ohlcv_chart_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate).Select(x => (double)x.Volume))
            //};
            //Volume_YFormatter = value => (value / 1000000).ToString();
            //volume_y_axis.LabelFormatter = Volume_YFormatter;
            //ohlcv_sc.Add(lineSeries);

            //shared x-axis - yet to complete
            InitialDateTime = start_date.SelectedDate.Value;
            OHLC_Labels = ohlcv_chart_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate).Select(x => x.Date.ToString("MM-dd-yy")).ToArray();

            ohlcv_chart_title.Content = $"{stock.Symbol} Daily Chart: {start_date.SelectedDate.Value.ToShortDateString()} to {end_date.SelectedDate.Value.ToShortDateString()}";
            ohlcv_chart.Series = ohlcv_sc;
            DataContext = this;
        }

        public string[] OHLC_Labels
        {
            get { return _ohlc_labels; }
            set
            {
                _ohlc_labels = value;
                OnPropertyChanged("OHLC_Labels");
            }
        }

        private void Go_Button_Click(object sender, RoutedEventArgs e) //refreshes grids based on new daterange
        {

            if (sa_choice.IsSelected)
            {                
                RefreshPieChart();
            }
            else if (ohlcv_choice.IsSelected) 
            {
                RefreshOHLCV_Chart();
            }
        }

        int i = 0;
        private async void Grid_choice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sa_choice.IsSelected)
            {
                ohlcv_panel.Visibility = Visibility.Hidden;
                trends_panel.Visibility = Visibility.Hidden;
                timeperiod_options.Visibility = Visibility.Collapsed;
                stockpicker.Visibility = Visibility.Collapsed;
                advanced_search_btn.Visibility = Visibility.Collapsed;

                start_date.SelectedDate = new DateTime(2020, 11, 25);
                end_date.SelectedDate = new DateTime(2020, 11, 25);
                RefreshPieChart();
                sa_panel.Visibility = Visibility.Visible;
                daterangepicker.Visibility = Visibility.Visible;
            }
            else if (ohlcv_choice.IsSelected) //still have yet to review
            {
                sa_panel.Visibility = Visibility.Hidden;
                trends_panel.Visibility = Visibility.Hidden;
                timeperiod_options.Visibility = Visibility.Collapsed;
                advanced_search_btn.Visibility = Visibility.Collapsed;

                //default stock selected??  in xaml? if not in xaml then set the combobox default value here 
                //set default dates
                start_date.SelectedDate = new DateTime(2020, 10, 20);
                end_date.SelectedDate = new DateTime(2020, 11, 20); //1 month worth of data
                stocks_selector.SelectedIndex = 0;
                //Stocks_selector_SelectionChanged(sender, e);
                ohlcv_panel.Visibility = Visibility.Visible;
                daterangepicker.Visibility = Visibility.Visible;
                stockpicker.Visibility = Visibility.Visible;


            }
            else if (trend_choice.IsSelected)
            {
                trends_panel.Visibility = Visibility.Visible;
                sa_panel.Visibility = Visibility.Hidden;
                ohlcv_panel.Visibility = Visibility.Hidden;
                daterangepicker.Visibility = Visibility.Collapsed;

                stocks_selector.SelectedIndex = -1;
                timeperiod_options.Visibility = Visibility.Visible;
                stockpicker.Visibility = Visibility.Visible;
                advanced_search_btn.Visibility = Visibility.Visible;

                if (i == 0)
                {
                    await Refresh_Trend_Scanner();
                    i++;
                }
                advanced_search_btn.IsEnabled = true;
                yr_1.IsSelected = true;
            }
        }        

        public async Task Refresh_Trend_Scanner()
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                scanner_progressBar.Value = 0;
                scanner_progressBar.Maximum = symbols.Count;

                await Task.Run(() =>
                {
                    //want this to be parallel but throwing an exception occasionally + final lists aren't = though they shd be 
                    //foreach (var symbol in symbols)
                    Parallel.ForEach(symbols, symbol =>
                    {
                        log.Info(symbol.Symbol);
                        Edit_Status_Label($"Loading {symbol.Symbol}...Time Elapsed: {stopwatch.ElapsedMilliseconds / 1000} s");
                        var stock_data = new List<ScannerFacade>();
                        using (var context = new NeeksDBEntities())
                        {
                            var end = context.HistoricalDatas.Where(x => x.StockID == symbol.ID).Max(x => x.Date);
                            var start = end.AddYears(-1);
                            stock_data = context.HistoricalDatas.Where(x => x.StockID == symbol.ID).Where(x => x.Date >= start && x.Date <= end).Select(x =>
                             new ScannerFacade { StockID = x.StockID, Date = x.Date, Close = x.Close }).OrderByDescending(x => x.Date).ToList();
                        }

                        DateTime min = new DateTime();
                        DateTime max = stock_data.Max(x => x.Date);

                        //1 yr
                        var _1yrData = stock_data.OrderBy(x => x.Date).ToList();
                        scanner_1yr.Add(CalculateTrendDirection(symbol, _1yrData));

                        //6 mo
                        min = max.AddMonths(-6);
                        var _6moData = stock_data.Where(x => x.Date >= min && x.Date <= max).OrderBy(x => x.Date).ToList();
                        scanner_6mo.Add(CalculateTrendDirection(symbol, _6moData));

                        //3 mo
                        min = max.AddMonths(-3);
                        var _3moData = stock_data.Where(x => x.Date >= min && x.Date <= max).OrderBy(x => x.Date).ToList();
                        scanner_3mo.Add(CalculateTrendDirection(symbol, _3moData));

                        //1 mo
                        min = max.AddMonths(-1);
                        var _1moData = stock_data.Where(x => x.Date >= min && x.Date <= max).OrderBy(x => x.Date).ToList();
                        scanner_1mo.Add(CalculateTrendDirection(symbol, _1moData));

                        Increment_Progress_Bar();

                    });
                });
                scanner_loadStatus.Content = $"All {symbols.Count} stocks loaded. Time Elapsed: {stopwatch.ElapsedMilliseconds / 1000} s";
            
            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException != null ? $"CURRENT EXCEPTION - {ex.ToString()} \n\nINNER EXCEPTION - {ex.InnerException.ToString()}" : ex.ToString());
                MessageBox.Show($"ERROR - {ex.Message}");
            }
        }

        private Scanner_DatagridsFacade CalculateTrendDirection(SymbolFacade stock, List<ScannerFacade> stock_data)
        {

            var x_vals = new double[stock_data.Count];
            var y_vals = new double[stock_data.Count];
            for (int i = 0; i < stock_data.Count; i++)
            {
                x_vals[i] = i;
                y_vals[i] = stock_data[i].Close;
            }

            LinearRegressions.LinReg(x_vals, y_vals, out var slope, out var y_int, out var r, out var r_squared, out var s);
            var predicted_y_vals = LinearRegressions.GeneratePredictedPts(x_vals, slope, y_int).Select(point => point[1]);
            var y_hat_length = predicted_y_vals.Max() - predicted_y_vals.Min();
            
            var slope_degrees = LinearRegressions.SlopeToDegrees(slope); 

            if (slope_degrees < consolidation_min_degrees)
            {
                return new Scanner_DatagridsFacade { Company = stock.Company, StockID = stock.ID, Symbol = stock.Symbol, Trend = TrendDirection.Downtrend };
            }
            else if (slope_degrees >= consolidation_min_degrees && slope_degrees <= consolidation_max_degrees)
            {
                 return new Scanner_DatagridsFacade { Company = stock.Company, StockID = stock.ID, Symbol = stock.Symbol, Trend = TrendDirection.Consolidating };
            }
            else if (slope_degrees > consolidation_max_degrees)
            {
                return new Scanner_DatagridsFacade { Company = stock.Company, StockID = stock.ID, Symbol = stock.Symbol, Trend = TrendDirection.Uptrend };
            }
            else
            {
                //probs gonna wanna handle issue diff here
                MessageBox.Show("The slope calculated is not valid.");
                throw new Exception($"The slope of value {slope_degrees} exceeded its range limits of -90 to 90.");
            }

        }

        public void Increment_Progress_Bar(int val = 1)
        {
            try
            {
                if (val == 0)
                {
                    MessageBox.Show("Cannot increment progress bar by 0.");
                    return;
                }
                Dispatcher.Invoke(() => (val == 1) ? scanner_progressBar.Value++ : scanner_progressBar.Value += val);

            }
            catch (Exception ex)
            {
                log.Error(ex.InnerException != null ? $"CURRENT EXCEPTION - {ex.ToString()} \n\nINNER EXCEPTION - {ex.InnerException.ToString()}" : ex.ToString());
                MessageBox.Show($"ERROR - {ex.Message}");
            }
        }

        public void Set_Progress_Bar_Max(int value)
        {
            Dispatcher.Invoke(() => scanner_progressBar.Maximum = value);
        }

        public void Edit_Status_Label(string msg)
        {
            Dispatcher.Invoke(() => scanner_loadStatus.Content = msg);
        }

        //int _prev_seleccted_index = -100;

        private void Stocks_selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (stocks_selector.SelectedIndex != _prev_seleccted_index && stocks_selector.SelectedIndex != -1 && start_date.SelectedDate.HasValue && end_date.SelectedDate.HasValue)        
            //{
            //RefreshOHLCV_Chart();
            //_prev_seleccted_index = stocks_selector.SelectedIndex;
            //}

            //if(stocks_selector.SelectedIndex == -1 && !start_date.SelectedDate.HasValue && !end_date.SelectedDate.HasValue)
            //{
            //    stocks_selector.SelectedIndex = 0;
            //    return;
            //}
            //else
            //{
            //    RefreshOHLCV_Chart();
            //}
            if (ohlcv_choice.IsSelected)
            {
                if (!start_date.SelectedDate.HasValue && !end_date.SelectedDate.HasValue || stocks_selector.SelectedIndex == -1)
                {
                    return;
                }
                RefreshOHLCV_Chart();
            }
            else if (trend_choice.IsSelected)
            {
                //need to change the selected stocks IsChartOpen property to true here
                var selection = (ComboBox)e.Source;
                var stock = (SymbolFacade)selection.SelectedItem;
                if(stock == null)
                {
                    return;
                }

                //open the new window
                //MAKE SURE THAT THE CORRECT SLOPE UNIT GETS PASSED INTO THE POPUP WINDOW
                //if isChartOpen == true, then message box and return
                StockLinRegChart linRegChart = new StockLinRegChart(stock, consolidation_min_degrees, consolidation_max_degrees);
                linRegChart.Show();
                //change property here
            }
        }

        private async void Configure_sloperange_Clicked(object sender, RoutedEventArgs e)
        {
            SlopeRangeConfiguration config_window = new SlopeRangeConfiguration(consolidation_min_degrees, consolidation_max_degrees);
            config_window.ShowDialog();

            if (!config_window.isCancel)
            {
                IsDegrees = config_window.isDegrees;

                //convert slope ranges to degrees if not already in degreees
                if (!IsDegrees)
                {
                    consolidation_min_degrees = LinearRegressions.SlopeToDegrees(config_window.min);
                    consolidation_max_degrees = LinearRegressions.SlopeToDegrees(config_window.max);
                }
                else
                {
                    consolidation_min_degrees = config_window.min;
                    consolidation_max_degrees = config_window.max;
                }

                //reset global lists
                scanner_1yr.ToList().Clear();
                scanner_6mo.ToList().Clear();
                scanner_3mo.ToList().Clear();
                scanner_1mo.ToList().Clear();

                //repopulate global lists using updated slope range
                await Refresh_Trend_Scanner();
                advanced_search_btn.IsEnabled = true;
                uptrend_grid.ItemsSource = null;
                consolidating_grid.ItemsSource = null;
                downtrend_grid.ItemsSource = null;

                timeperiod_selector.SelectedIndex = -1;
                yr_1.IsSelected = true;
                RepopulateAdvancedSearchInfo = true;

            }
        }

        private void Timeperiod_selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (yr_1.IsSelected)
            {
                uptrend_grid.ItemsSource = scanner_1yr.Where(x => x.Trend == TrendDirection.Uptrend);
                consolidating_grid.ItemsSource = scanner_1yr.Where(x => x.Trend == TrendDirection.Consolidating);
                downtrend_grid.ItemsSource = scanner_1yr.Where(x => x.Trend == TrendDirection.Downtrend);
            }
            else if (mo_6.IsSelected)
            {
                uptrend_grid.ItemsSource = scanner_6mo.Where(x => x.Trend == TrendDirection.Uptrend);
                consolidating_grid.ItemsSource = scanner_6mo.Where(x => x.Trend == TrendDirection.Consolidating);
                downtrend_grid.ItemsSource = scanner_6mo.Where(x => x.Trend == TrendDirection.Downtrend);
            }
            else if (mo_3.IsSelected)
            {
                uptrend_grid.ItemsSource = scanner_3mo.Where(x => x.Trend == TrendDirection.Uptrend);
                consolidating_grid.ItemsSource = scanner_3mo.Where(x => x.Trend == TrendDirection.Consolidating);
                downtrend_grid.ItemsSource = scanner_3mo.Where(x => x.Trend == TrendDirection.Downtrend);
            }
            else if (mo_1.IsSelected)
            {
                uptrend_grid.ItemsSource = scanner_1mo.Where(x => x.Trend == TrendDirection.Uptrend);
                consolidating_grid.ItemsSource = scanner_1mo.Where(x => x.Trend == TrendDirection.Consolidating);
                downtrend_grid.ItemsSource = scanner_1mo.Where(x => x.Trend == TrendDirection.Downtrend);
            }
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //change IsChartOpen property here too
            var selection = (DataGridRow)e.Source;
            var temp = (Scanner_DatagridsFacade)selection.DataContext;

            if (temp == null)
            {
                return;
            }
            var stock = new SymbolFacade { ID = temp.StockID, Symbol = temp.Symbol, Company = temp.Company };

            //open the new window
            //MAKE SURE THAT THE CORRECT SLOPE UNIT GETS PASSED INTO THE POPUP WINDOW
            StockLinRegChart linRegChart = new StockLinRegChart(stock, consolidation_min_degrees, consolidation_max_degrees);
            linRegChart.Show();

        }

        bool RepopulateAdvancedSearchInfo = true;
        public List<AdvancedSearchInfo> advancedSearchInfos = new List<AdvancedSearchInfo>();
        //for trend scanner
        private void Advanced_search_btn_Click(object sender, RoutedEventArgs e)
        {

            if (RepopulateAdvancedSearchInfo)
            {
                advancedSearchInfos = new List<AdvancedSearchInfo>(symbols.Select(x => new AdvancedSearchInfo { StockID = x.ID, Company = x.Company, Symbol = x.Symbol }));

                foreach (var item in scanner_1yr)
                {
                    advancedSearchInfos.FirstOrDefault(x => x.StockID == item.StockID)._1yrTrend = item.Trend;

                }
                foreach (var item in scanner_6mo)
                {
                    advancedSearchInfos.FirstOrDefault(x => x.StockID == item.StockID)._6moTrend = item.Trend;

                }
                foreach (var item in scanner_3mo)
                {
                    advancedSearchInfos.FirstOrDefault(x => x.StockID == item.StockID)._3moTrend = item.Trend;

                }
                foreach (var item in scanner_1mo)
                {
                    advancedSearchInfos.FirstOrDefault(x => x.StockID == item.StockID)._1moTrend = item.Trend;
                }
                //not the biggest fan of how this list was populated 
                RepopulateAdvancedSearchInfo = false;
            }

            AdvancedSearchWindow window = new AdvancedSearchWindow(consolidation_min_degrees, consolidation_max_degrees, advancedSearchInfos);
            window.Show();
            //use Show() but if opened once then don't allow after that
        }

    }

    public class AdvancedSearchInfo
    {
        public long StockID { get; set; }
        public string Symbol { get; set; }
        public string Company { get; set; }
        public TrendDirection _1yrTrend { get; set; }
        public TrendDirection _6moTrend { get; set; }
        public TrendDirection _3moTrend { get; set; }
        public TrendDirection _1moTrend { get; set; }

    }



}

