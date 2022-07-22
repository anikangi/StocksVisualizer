using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utilities;
using ExtensionMethods;

namespace StocksVisualizerUI
{
    /// <summary>
    /// Interaction logic for ModelessChartPopUp.xaml
    /// </summary>
    public class Chart_Facade
    {
        public double Close { get; set; }
        public DateTime Date { get; set; }
    }

    public partial class ModelessChartPopUp : Window
    {
        UIStockInfo selectedStockInfo;
        int period = 2000;
        List<Chart_Facade> raw_data = new List<Chart_Facade>();
        List<Chart_Facade> standardized_data = new List<Chart_Facade>();

        List<Chart_Facade> rawSPY_data = new List<Chart_Facade>();
        List<Chart_Facade> SPY_data = new List<Chart_Facade>();
        List<Chart_Facade> rawDIA_data = new List<Chart_Facade>();
        List<Chart_Facade> DIA_data = new List<Chart_Facade>();
        List<Chart_Facade> rawQQQ_data = new List<Chart_Facade>();
        List<Chart_Facade> QQQ_data = new List<Chart_Facade>();

        public ModelessChartPopUp(UIStockInfo selected_stock_info)
        {
            InitializeComponent();
            selectedStockInfo = selected_stock_info;
            this.Loaded += new RoutedEventHandler(Window_Loaded);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
            await Task.Run(() => 
            {
                using (var context = new NeeksDBEntities())
                {
                    raw_data = context.HistoricalDatas.Where(x => x.StockID == selectedStockInfo.ID).Select( x =>  new Chart_Facade {Close = x.Close, Date = x.Date }).ToList();
                }

                Dispatcher.Invoke(() => 
                {                   
                    chart.Title = $"{selectedStockInfo.Symbol}: Closing Price vs. Time";
                    show_original.IsChecked = true;
                });                
            });

            
        }
        
        private async Task RefreshGrid(List<Chart_Facade> data, bool useDefaultDates=false)
        {
            
            var min = data.Min(x => x.Date);
            var max = data.Max(x => x.Date);

            if (useDefaultDates)
            {
                start_date.SelectedDate = min;
                end_date.SelectedDate = max;
            }

            var from = start_date.SelectedDate;
            var to =  end_date.SelectedDate;


            await Task.Run(() => 
            {
                List<Chart_Facade> chart_data = data.Where(x => x.Date >= from && x.Date <= to).ToList();

                Dispatcher.Invoke(() => 
                {
                    histData_graph.ItemsSource = chart_data;
                    start_date.DisplayDateStart = min;
                    start_date.DisplayDateEnd = max;
                    end_date.DisplayDateStart = min;
                    end_date.DisplayDateEnd = max;
                });

            });
            

         
        }

        private void Start_date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if(start_date.SelectedDate.Value > end_date.SelectedDate.Value)
            {
                MessageBox.Show("End date cannot be after start date.");
                return;
            }
            IEnumerable<Chart_Facade> selection;
            if (show_standardized.IsChecked.Value)
            {
                selection = (end_date.SelectedDate == null) ? standardized_data.Where(x => x.Date >= start_date.SelectedDate):
                    standardized_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            }
            else
            {
                selection = (end_date.SelectedDate == null) ? raw_data.Where(x => x.Date >= start_date.SelectedDate) :
                    raw_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            }
            histData_graph.ItemsSource = selection;

            if (sp_500.IsChecked.Value)
            { Sp_500_Checked(sender, e); }

            if (djia.IsChecked.Value)
            { Djia_Checked(sender, e); }

            if (qqq.IsChecked.Value)
            { Qqq_Checked(sender, e); }
        }

        private void End_date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (start_date.SelectedDate.Value > end_date.SelectedDate.Value)
            {
                MessageBox.Show("End date cannot be after start date.");
                return;
            }
            IEnumerable<Chart_Facade> selection;
            if (show_standardized.IsChecked.Value)
            {
                selection = (start_date.SelectedDate == null) ? standardized_data.Where(x => x.Date <= end_date.SelectedDate): 
                    standardized_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            }
            else
            {
                selection =(start_date.SelectedDate == null) ? raw_data.Where(x => x.Date <= end_date.SelectedDate) :
                    raw_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            }
            histData_graph.ItemsSource = selection;

            if (sp_500.IsChecked.Value)
            { Sp_500_Checked(sender, e); }

            if (djia.IsChecked.Value)
            { Djia_Checked(sender, e); }

            if (qqq.IsChecked.Value)
            { Qqq_Checked(sender, e); }
        }

        private void Close_config_tab_Click(object sender, RoutedEventArgs e)
        {
            if (configuration_tab.Visibility == Visibility.Visible)
            {
                configuration_tab.Visibility = Visibility.Collapsed;
                main_grid.SetValue(Grid.ColumnSpanProperty, 2);
                config_tab_btn.RenderTransform = new RotateTransform(180);
            }
            else
            {
                configuration_tab.Visibility = Visibility.Visible;
                main_grid.SetValue(Grid.ColumnSpanProperty, 1);
                config_tab_btn.RenderTransform = new RotateTransform(0);

            }

        }

        private List<Chart_Facade> ConvertToStandardizedForm(List<Chart_Facade> original_data, int period)
        {
            var queue = new Queue<double>(period);
            var standardized_data = new List<Chart_Facade>();
            for (int i = 0; i < original_data.Count; i++)
            {
                var original_pt = original_data[i].Close;
                if(i < period-1)
                {
                    queue.Enqueue(original_pt);
                    continue;
                }

                queue.Enqueue(original_pt);
                double mean = queue.Average();
                double sd = queue.StandardDev();
                double standardized_pt = Normalizations.StandardizeVal(original_pt, sd, mean);
                //shd i round standardized_pt??
                standardized_data.Add(new Chart_Facade { Close = standardized_pt, Date = original_data[i].Date });
                queue.Dequeue();
            }

            return standardized_data;           
        }

        private void Sp_500_Checked(object sender, RoutedEventArgs e)
        {
            if (!rawSPY_data.Any())
            {
                using (var context = new NeeksDBEntities())
                {
                    rawSPY_data = context.HistoricalDatas.Where(x => x.StockID == 7666).Select(x => new Chart_Facade { Close = x.Close, Date = x.Date }).ToList();
                }
            }
            
            SPY_data = ConvertToStandardizedForm(rawSPY_data, period);
            spy_graph.ItemsSource = SPY_data.Where(x=> x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            spy_graph.Visibility = Visibility.Visible;
            
        }

        private void Sp_500_Unchecked(object sender, RoutedEventArgs e)
        {
            spy_graph.Visibility = Visibility.Collapsed;
        }

        private void Djia_Checked(object sender, RoutedEventArgs e)
        {
            if (!rawDIA_data.Any())
            {
                using (var context = new NeeksDBEntities())
                {
                    rawDIA_data = context.HistoricalDatas.Where(x => x.StockID == 2145).Select(x => new Chart_Facade { Close = x.Close, Date = x.Date }).ToList();
                }
            }

            DIA_data = ConvertToStandardizedForm(rawDIA_data, period);
            djia_graph.ItemsSource = DIA_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            djia_graph.Visibility = Visibility.Visible;
        }

        private void Djia_Unchecked(object sender, RoutedEventArgs e)
        {
            djia_graph.Visibility = Visibility.Collapsed;
        }

        private void Qqq_Checked(object sender, RoutedEventArgs e)
        {
            if (!rawQQQ_data.Any())
            {
                using (var context = new NeeksDBEntities())
                {
                    rawQQQ_data = context.HistoricalDatas.Where(x => x.StockID == 6781).Select(x => new Chart_Facade { Close = x.Close, Date = x.Date }).ToList();
                }
            }

            QQQ_data = ConvertToStandardizedForm(rawQQQ_data, period);
            qqq_graph.ItemsSource = QQQ_data.Where(x => x.Date >= start_date.SelectedDate && x.Date <= end_date.SelectedDate);
            qqq_graph.Visibility = Visibility.Visible;
        }

        private void Qqq_Unchecked(object sender, RoutedEventArgs e)
        {
            qqq_graph.Visibility = Visibility.Collapsed;
        }

        private async void Period_selector_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var input = period_selector.Text;
                if (string.IsNullOrEmpty(input))
                {
                    MessageBox.Show("Please choose a period.");
                    return;
                }
                bool is_successful = int.TryParse(input, out period);
                if (!is_successful)
                {
                    MessageBox.Show("Please type a number.");
                    return;
                }
                if (period == 0)
                {
                    MessageBox.Show("Period must be greater than 0.");
                    return;
                }

                standardized_data = ConvertToStandardizedForm(raw_data, period);

                if ((!start_date.SelectedDate.HasValue) && (!end_date.SelectedDate.HasValue))
                {
                   await  RefreshGrid(standardized_data, true);
                }
                else
                {
                   await  RefreshGrid(standardized_data);
                }

                if (sp_500.IsChecked.Value)
                { Sp_500_Checked(sender, e); }

                if (djia.IsChecked.Value)
                { Djia_Checked(sender, e); }

                if (qqq.IsChecked.Value)
                { Qqq_Checked(sender, e); }
            }
        }

        private async void Show_original_Checked(object sender, RoutedEventArgs e)
        {
            benchmarks_gb.IsEnabled = false;
            period_gb.IsEnabled = false;
            spy_graph.Visibility = Visibility.Collapsed;
            djia_graph.Visibility = Visibility.Collapsed;
            qqq_graph.Visibility = Visibility.Collapsed;

            if((!start_date.SelectedDate.HasValue) && (!end_date.SelectedDate.HasValue))
            {
               await RefreshGrid(raw_data,true);
            }
            else
            {
               await RefreshGrid(raw_data);
            }

        }

        private async void Show_standardized_Checked(object sender, RoutedEventArgs e)
        {
            benchmarks_gb.IsEnabled = true;
            period_gb.IsEnabled = true;
            standardized_data = ConvertToStandardizedForm(raw_data, period);

            if (sp_500.IsChecked.Value) { spy_graph.Visibility = Visibility.Visible; }
            if (djia.IsChecked.Value) { djia_graph.Visibility = Visibility.Visible; }
            if (qqq.IsChecked.Value) { qqq_graph.Visibility = Visibility.Visible; }

            if ((!start_date.SelectedDate.HasValue) && (!end_date.SelectedDate.HasValue))
            {
                await RefreshGrid(standardized_data, true);
            }
            else
            {
               await RefreshGrid(standardized_data);
            }
        }
    }
}
