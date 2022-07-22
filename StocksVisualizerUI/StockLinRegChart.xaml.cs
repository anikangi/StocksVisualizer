using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Utilities;
namespace StocksVisualizerUI
{
    /// <summary>
    /// Interaction logic for StockLinRegChart.xaml
    /// </summary>
    /// 
    public class LinRegInfo
    {
        public double m { get; set; }
        public double m_degrees { get; set; }
        public double b { get; set; }
        public double r { get; set; }
        public double rSquared { get; set; }
        public double s { get; set; }
        public TrendDirection trendProjection { get; set; }
        public Dictionary<DateTime, double> predictedPoints { get; set; }
        //or can use a double array whichever u feel works better to make sure the dates correlate
        public DateTime minDate { get; set; }
        public DateTime maxDate { get; set; }

    }

    public partial class StockLinRegChart : Window
    {
        //series collection items
        SeriesCollection seriesCollection = new SeriesCollection();
        public Func<double, string> YFormatter { get; set; }

        string[] xAxisDates;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string[] XAxisDates
        {
            get { return xAxisDates; }
            set
            {
                xAxisDates = value;
                OnPropertyChanged();
            }
        }

        //global data
        Dictionary<DateTime, double> actualPoints = new Dictionary<DateTime, double>();
        LinRegInfo linRegInfo_1YR = new LinRegInfo();
        LinRegInfo linRegInfo_6MO = new LinRegInfo();
        LinRegInfo linRegInfo_3MO = new LinRegInfo();
        LinRegInfo linRegInfo_1MO = new LinRegInfo();

        //variables passed from dashboard window
        SymbolFacade stock = new SymbolFacade();
        double consolidationMin;
        double consolidationMax;
        public StockLinRegChart(SymbolFacade symbol, double consolidation_min_degrees, double consolidation_max_degrees)
        {
            InitializeComponent();

            stock = symbol;
            consolidationMin = consolidation_min_degrees;
            consolidationMax = consolidation_max_degrees;
            this.Loaded += new RoutedEventHandler(Window_Loaded);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) 
        {
            linreg_chart_title.Content = $"{stock.Symbol}: Closing Price vs. Time";

            using (var context = new NeeksDBEntities())
            {
                var end = context.HistoricalDatas.Where(x => x.StockID == stock.ID).Max(x => x.Date);
                var start = end.AddYears(-1);
                actualPoints = context.HistoricalDatas.Where(x => x.StockID == stock.ID).Where(x => x.Date >= start && x.Date <= end).ToDictionary(x=> x.Date, x=> x.Close);
            }


            DateTime min = new DateTime();
            DateTime max = actualPoints.Max(x => x.Key);

            // CHECK IF ORDER BY IS EVEN NEEDED
            //1 yr
            var _1yrData = actualPoints.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value); 
            linRegInfo_1YR = CalculateLinRegInfo(_1yrData);
            tp_1YR.Content = linRegInfo_1YR.trendProjection.ToString();
            tp_1YR.Foreground = SetTrendProjectionColor(linRegInfo_1YR.trendProjection);

            //6 mo
            min = max.AddMonths(-6);
            var _6moData = actualPoints.Where(x => x.Key >= min && x.Key <= max).OrderBy(x => x.Key).ToDictionary(x=> x.Key, x=> x.Value); 
            linRegInfo_6MO = CalculateLinRegInfo(_6moData);
            tp_6MO.Content = linRegInfo_6MO.trendProjection.ToString();
            tp_6MO.Foreground = SetTrendProjectionColor(linRegInfo_6MO.trendProjection);


            //3 mo
            min = max.AddMonths(-3);
            var _3moData = actualPoints.Where(x => x.Key >= min && x.Key <= max).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            linRegInfo_3MO = CalculateLinRegInfo(_3moData);
            tp_3MO.Content = linRegInfo_3MO.trendProjection.ToString();
            tp_3MO.Foreground = SetTrendProjectionColor(linRegInfo_3MO.trendProjection);


            //1 mo
            min = max.AddMonths(-1);
            var _1moData = actualPoints.Where(x => x.Key >= min && x.Key <= max).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            linRegInfo_1MO = CalculateLinRegInfo(_1moData);
            tp_1MO.Content = linRegInfo_1MO.trendProjection.ToString();
            tp_1MO.Foreground = SetTrendProjectionColor(linRegInfo_1MO.trendProjection);


            #region set all constant properties for the 2 series within seriesCollection
            //line smoothness? 

            var closingPrice_series = new LineSeries
            {
                Title = "Closing Prices",
                Name = "closingPrice_series",
                PointGeometrySize = 0
            };
            var linReg_series = new LineSeries
            {
                Title = "Predicted Prices (LSRL)",
                Name = "linReg_series",
                PointGeometrySize = 0
            };
            seriesCollection.Add(closingPrice_series);
            seriesCollection.Add(linReg_series);

            YFormatter = value => value.ToString("C");

            stock_chart.Series = seriesCollection;
            DataContext = this;

            selection_1yr.IsChecked = true;
            Show_LSRL.IsChecked = true;
            #endregion

        }

        private Brush SetTrendProjectionColor(TrendDirection trendProjection)
        {
            var result = Brushes.Transparent;
            switch (trendProjection)
            {
                case TrendDirection.Uptrend:
                     result = Brushes.ForestGreen;
                    break;
                case TrendDirection.Downtrend:
                    result=  Brushes.Red;
                    break;
                case TrendDirection.Consolidating:
                    result = Brushes.CornflowerBlue;
                    break;
            }
            return result;
        }

        private LinRegInfo CalculateLinRegInfo(Dictionary<DateTime, double> points)
        {
            var result = new LinRegInfo();

            var x_vals = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                x_vals[i] = i;
            }
            double[] y_vals = points.Values.ToArray();

            LinearRegressions.LinReg(x_vals, y_vals, out var slope, out var y_int, out var r, out var r_squared, out var s);
            result.m = slope;
            result.b = y_int;
            result.r = r;
            result.rSquared = r_squared;
            result.s = s;
            result.m_degrees = LinearRegressions.SlopeToDegrees(slope);


            var predicted_y_vals = LinearRegressions.GeneratePredictedPts(x_vals, slope, y_int).Select(point => point[1]).ToArray();

            var temp_list = new Dictionary<DateTime, double>();
            for (int i = 0; i < points.Count(); i++)
            {
                temp_list.Add(points.Keys.ToList()[i], predicted_y_vals[i]);

            }
            result.predictedPoints = temp_list;

            result.minDate = temp_list.Min(x => x.Key);
            result.maxDate = temp_list.Max(x => x.Key);

            var slope_degrees = LinearRegressions.SlopeToDegrees(slope);

            if (slope_degrees < consolidationMin)
            {
                result.trendProjection = TrendDirection.Downtrend;
            }
            else if (slope_degrees >= consolidationMin && slope_degrees <= consolidationMax)
            {
                result.trendProjection = TrendDirection.Consolidating;
            }
            else if (slope_degrees > consolidationMax)
            {
                result.trendProjection = TrendDirection.Uptrend;
            }
            return result;
        }

        private void Selection_1yr_Checked(object sender, RoutedEventArgs e)
        {
            SetGroupBox1(linRegInfo_1YR);
            seriesCollection[0].Values = new ChartValues<double>(actualPoints.Values);
            seriesCollection[1].Values = new ChartValues<double>(linRegInfo_1YR.predictedPoints.Values);
            XAxisDates = linRegInfo_1YR.predictedPoints.Select(x => x.Key.ToString("MM-dd-yy")).ToArray();
            x_axis.Labels = XAxisDates;
            DataContext = this;

        }

        private void Selection_6mo_Checked(object sender, RoutedEventArgs e)
        {
            SetGroupBox1(linRegInfo_6MO);
            seriesCollection[0].Values = new ChartValues<double>(actualPoints.Where(x => x.Key >= linRegInfo_6MO.minDate && x.Key <= linRegInfo_6MO.maxDate).Select(x=> x.Value));
            seriesCollection[1].Values = new ChartValues<double>(linRegInfo_6MO.predictedPoints.Values);
            XAxisDates = linRegInfo_6MO.predictedPoints.Select(x => x.Key.ToString("MM-dd-yy")).ToArray();
            x_axis.Labels = XAxisDates;
            DataContext = this;


        }

        private void Selection_3mo_Checked(object sender, RoutedEventArgs e)
        {
            SetGroupBox1(linRegInfo_3MO);
            seriesCollection[0].Values = new ChartValues<double>(actualPoints.Where(x => x.Key >= linRegInfo_3MO.minDate && x.Key <= linRegInfo_3MO.maxDate).Select(x => x.Value));
            seriesCollection[1].Values = new ChartValues<double>(linRegInfo_3MO.predictedPoints.Values);
            XAxisDates = linRegInfo_3MO.predictedPoints.Select(x => x.Key.ToString("MM-dd-yy")).ToArray();
            x_axis.Labels = XAxisDates;
            DataContext = this;


        }

        private void Selection_1mo_Checked(object sender, RoutedEventArgs e)
        {
            SetGroupBox1(linRegInfo_1MO);
            seriesCollection[0].Values = new ChartValues<double>(actualPoints.Where(x => x.Key >= linRegInfo_1MO.minDate && x.Key <= linRegInfo_1MO.maxDate).Select(x => x.Value));
            seriesCollection[1].Values = new ChartValues<double>(linRegInfo_1MO.predictedPoints.Values);
            XAxisDates = linRegInfo_1MO.predictedPoints.Select(x => x.Key.ToString("MM-dd-yy")).ToArray();
            x_axis.Labels = XAxisDates;
            DataContext = this;

        }

        private void SetGroupBox1(LinRegInfo info)
        {
            LSRSL_equation.Text = $"{Math.Round(info.m, 3)}x + {Math.Round(info.b, 3)}";
            r.Text = $"r = {Math.Round(info.r, 3).ToString()}";
            r_squared.Text = $"r² = {Math.Round(info.rSquared, 3).ToString()}";
            m.Text = $"m = {Math.Round(info.m, 3).ToString()}";
            m_degrees.Text = $"m (°) = {Math.Round(info.m_degrees, 1).ToString()}°";

            b.Text = $"b = ${Math.Round(info.b, 3).ToString()}";
            s.Text = $"s = {Math.Round(info.s, 3).ToString()}";
        }

        private void Show_LSRL_Checked(object sender, RoutedEventArgs e)
        {
            //make sure the y-axis (and x-axis once thats done) adjusts as well
            ((LineSeries)seriesCollection[1]).Visibility = Visibility.Visible;

        }

        private void Show_LSRL_Unchecked(object sender, RoutedEventArgs e)
        {
           //make sure the y-axis (and x-axis once thats done) adjusts as well
            ((LineSeries)seriesCollection[1]).Visibility = Visibility.Collapsed;

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
