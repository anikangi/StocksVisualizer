using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace StocksVisualizerUI
{
    /// <summary>
    /// Interaction logic for AdvancedSearchWindow.xaml
    /// </summary>
    public partial class AdvancedSearchWindow : Window
    {
        double min;
        double max;
        public List<AdvancedSearchInfo> datas = new List<AdvancedSearchInfo>();
        //public List<Scanner_DatagridsFacade> scanner_1yr = new List<Scanner_DatagridsFacade>();
        //public List<Scanner_DatagridsFacade> scanner_6mo = new List<Scanner_DatagridsFacade>();
        //public List<Scanner_DatagridsFacade> scanner_3mo = new List<Scanner_DatagridsFacade>();
        //public List<Scanner_DatagridsFacade> scanner_1mo = new List<Scanner_DatagridsFacade>();

        public AdvancedSearchWindow(
            double consolidation_min_degrees, double consolidation_max_degrees, List<AdvancedSearchInfo> list)
        {
            InitializeComponent();
            min = consolidation_min_degrees;
            max = consolidation_max_degrees;
            datas = list;
            //components
            /*
             * 1. search bar (4 textblocks w/ combobox) with ENTER button 
             * 6. bugs found/final touches
             * ? only allow one popup? how to handle this
             */
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //change IsChartOpen property here too?
            var selection = (DataGridRow)e.Source;
            var temp = (AdvancedSearchInfo)selection.DataContext;

            if (temp == null)
            {
                return;
            }
            var stock = new SymbolFacade { ID = temp.StockID, Symbol = temp.Symbol, Company = temp.Company };

            //open the new window
            //MAKE SURE THAT THE CORRECT SLOPE UNIT GETS PASSED INTO THE POPUP WINDOW
            StockLinRegChart linRegChart = new StockLinRegChart(stock, min, max);
            linRegChart.Show();

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _1yrTrend.SelectedIndex = -1;
            _6moTrend.SelectedIndex = -1;
            _3moTrend.SelectedIndex = -1;
            _1moTrend.SelectedIndex = -1;

            filtered_grid.ItemsSource = null;
            empty_datagrid_msg.Visibility = Visibility.Collapsed;           
            filtered_list.Clear();
            stocks_search.Text = string.Empty;


            amtTrades_symbol.SelectedIndex = 0;
            stDev_symbol.SelectedIndex = 0;
            stDev_filter.IsChecked = false;
            tradesAmt_filter.IsChecked = false;
            amt_sample_days.Text = "1-35";
            amt_sample_days.Foreground = Brushes.Gray;
            amt_sample_days.FontStyle = FontStyles.Normal;



        }

        List<AdvancedSearchInfo> filtered_list = new List<AdvancedSearchInfo>();

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            empty_datagrid_msg.Visibility = Visibility.Collapsed;
            #region trend direction criteria
            var _1yrSelection = (_1yrTrend.SelectedIndex == -1) ? TrendDirection.None : (TrendDirection)_1yrTrend.SelectedIndex; 
            var _6moSelection = (_6moTrend.SelectedIndex == -1) ? TrendDirection.None : (TrendDirection)_6moTrend.SelectedIndex;
            var _3moSelection = (_3moTrend.SelectedIndex == -1) ? TrendDirection.None : (TrendDirection)_3moTrend.SelectedIndex;
            var _1moSelection = (_1moTrend.SelectedIndex == -1) ? TrendDirection.None : (TrendDirection)_1moTrend.SelectedIndex;

            if (!stDev_filter.IsChecked.Value && !tradesAmt_filter.IsChecked.Value && _1yrSelection <= TrendDirection.None && 
                _6moSelection <= TrendDirection.None && _3moSelection <= TrendDirection.None && _1moSelection <= TrendDirection.None)
            {
                MessageBox.Show("Please select criteria to filter stocks.");
                return;
            }

            filtered_list = datas;

            if (_1yrSelection != TrendDirection.None)
            {
                filtered_list = filtered_list.Where(x => x._1yrTrend == _1yrSelection).ToList();
            }
            if (_6moSelection != TrendDirection.None)
            {
                filtered_list = filtered_list.Where(x=> x._6moTrend == _6moSelection).ToList();
            }
            if (_3moSelection != TrendDirection.None)
            {
                filtered_list = filtered_list.Where(x => x._3moTrend == _3moSelection).ToList();
            }
            if (_1moSelection != TrendDirection.None)
            {
                filtered_list = filtered_list.Where(x => x._1moTrend== _1moSelection).ToList();
            }
            #endregion

            //checkbox criteria 
            if (stDev_filter.IsChecked.Value)
            {
                filtered_list = SortByStDev(filtered_list);
            }
            if (tradesAmt_filter.IsChecked.Value)
            {
                filtered_list = SortByAmtTrades(filtered_list);
            }


            if (!filtered_list.Any())
            {
                SoundPlayer soundPlayer = new SoundPlayer();
                soundPlayer.PlaySync();
                empty_datagrid_msg.Visibility = Visibility.Visible; //text: "No stocks fit the given criteria."
            }

            filtered_grid.ItemsSource = null;
            filtered_grid.ItemsSource = filtered_list;
        }

        public List<AdvancedSearchInfo> SortByAmtTrades(List<AdvancedSearchInfo> filteredList)
        {
            var results = new List<AdvancedSearchInfo>();
            foreach (var item in filteredList)
            {
                //need to make facade class later bc we don't need all columns
                var sampleStats = new List<SampleStat>();
                using (var context = new NeeksDBEntities())
                {
                    sampleStats = context.SampleStats.Where(x => x.StockID == item.StockID).OrderByDescending(x => x.Date).Take(sample_period).ToList();
                }

                if (!sampleStats.Any() || (sampleStats.Count < sample_period))
                {
                    continue;
                }

                var max_date = sampleStats.Max(x => x.Date);
                var lastDay_amtTrades = sampleStats.FirstOrDefault(x => x.Date == max_date).Total_AmtOfTrades;
                var avg_amtTrades = sampleStats.Average(x => x.Total_AmtOfTrades);

                //combobox defines the if block
                switch (amtTrades_symbol.SelectedIndex)
                {
                    case 0:
                        if (lastDay_amtTrades > avg_amtTrades)
                        {
                            results.Add(item);
                        }
                        break;
                    case 1:
                        if (lastDay_amtTrades < avg_amtTrades)
                        {
                            results.Add(item);
                        }
                        break;
                    case 2:
                        if (lastDay_amtTrades == avg_amtTrades)
                        {
                            results.Add(item);
                        }
                        break;
                }

            }

            
            return results;

        }

        public List<AdvancedSearchInfo> SortByStDev(List<AdvancedSearchInfo> filteredList)
        {
            var results = new List<AdvancedSearchInfo>();
            foreach (var item in filteredList)
            {
                //need to make facade class later bc we don't need all columns
                var sampleStats = new List<SampleStat>();
                using(var context = new NeeksDBEntities())
                {
                    sampleStats = context.SampleStats.Where(x=> x.StockID == item.StockID).OrderByDescending(x=> x.Date).Take(sample_period).ToList();
                }

                if (!sampleStats.Any() || (sampleStats.Count < sample_period))
                {
                    continue;
                }

                var max_date = sampleStats.Max(x=> x.Date);
                var lastDay_stDev = sampleStats.FirstOrDefault(x => x.Date == max_date).StDev;
                var avg_stDev = sampleStats.Average(x => x.StDev);

                //combobox defines the if block
                switch (stDev_symbol.SelectedIndex)
                {
                    case 0:
                        if(lastDay_stDev > avg_stDev)
                        {
                            results.Add(item);
                        }
                        break;
                    case 1:
                        if (lastDay_stDev < avg_stDev)
                        {
                            results.Add(item);
                        }
                        break;
                    case 2:
                        if (lastDay_stDev == avg_stDev)
                        {
                            results.Add(item);
                        }
                        break;
                }
            }
            return results;
        }

        private void Stocks_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (filtered_list.Any())
            {
                var choice = stocks_search.Text;
                filtered_grid.ItemsSource = filtered_list.Where(x => x.Symbol.StartsWith(choice.ToUpper()));
            }

        }

        #region pop-up methods
        bool is1_clicked = false, is2_clicked = false;
        private void Info_btn1_MouseEnter(object sender, MouseEventArgs e)
        {
            if (is1_clicked)
            {
                return;
            }
            popup1.IsOpen = true;
        }

        private void Info_btn1_MouseLeave(object sender, MouseEventArgs e)
        {
            if (is1_clicked)
            {
                return;
            }
            popup1.IsOpen = false;
        }

        private void Info_btn2_MouseEnter(object sender, MouseEventArgs e)
        {
            if (is2_clicked)
            {
                return;
            }
            popup2.IsOpen = true;

        }

        private void Info_btn2_MouseLeave(object sender, MouseEventArgs e)
        {
            if (is2_clicked)
            {
                return;
            }
            popup2.IsOpen = false;
        }

        private void Info_btn1_Click(object sender, RoutedEventArgs e)
        {
            if (is1_clicked)
            {
                popup1.IsOpen = false;
                is1_clicked = false;
            }
            else
            {
                popup1.IsOpen = true;
                is1_clicked = true;
            }
        }


        private void Info_btn2_Click(object sender, RoutedEventArgs e)
        {
            if (is2_clicked)
            {
                popup2.IsOpen = false;
                is2_clicked = false;
            }
            else
            {
                popup2.IsOpen = true;
                is2_clicked = true;
            }

        }
        #endregion

        int sample_period = 35;
        private void Amt_sample_days_TextChanged(object sender, TextChangedEventArgs e)
        {
            error_label.Text = "";
            if (amt_sample_days.Text == "1-35" || string.IsNullOrEmpty(amt_sample_days.Text))
            {
                sample_period = 35;
                return;
            }
            bool isParseSuccessful = int.TryParse(amt_sample_days.Text, out var variable);
            if (!isParseSuccessful || variable <= 0 || variable > 35)
            {
                error_label.Text = "Sample daterange must be a number from 1-35.";
                sample_period = 35;
                return;
            }
            sample_period = variable;
        }

        private void Amt_sample_days_GotFocus(object sender, RoutedEventArgs e)
        {
            if(amt_sample_days.Text == "1-35")
            {
                amt_sample_days.Foreground = Brushes.Black;
                amt_sample_days.FontStyle = FontStyles.Normal;
                amt_sample_days.Text = "";
            }
        }

        private void Amt_sample_days_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(error_label.Text) || string.IsNullOrEmpty(amt_sample_days.Text))
            {
                amt_sample_days.Text = "1-35";
                amt_sample_days.Foreground = Brushes.Gray;
                amt_sample_days.FontStyle = FontStyles.Italic;
            }
        }
    }
}
