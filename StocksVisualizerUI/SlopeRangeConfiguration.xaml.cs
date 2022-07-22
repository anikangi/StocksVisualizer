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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StocksVisualizerUI
{
    /// <summary>
    /// Interaction logic for SlopeRangeConfiguration.xaml
    /// </summary>
    public partial class SlopeRangeConfiguration : Window
    {
        public bool isCancel;
        public double min;
        public double max;
        public bool isDegrees = true;
    
        public SlopeRangeConfiguration(double consolidation_min_degrees, double consolidation_max_degrees)
        {
            min = consolidation_min_degrees;
            max = consolidation_max_degrees;
            InitializeComponent();
            consolidation_min.Text = min.ToString();
            consolidation_max.Text = max.ToString();

        }

        int i = 0;
        private void Degrees_Checked(object sender, RoutedEventArgs e)
        {
            if (i < 2)
            {
                i++;
                return;
            }

            isDegrees = true;
            uptrend_range.Text = $"{max + 1}° to 90°";
            downtrend_range.Text = $"-90° to {min - 1}°";
            consolidation_min.Text = min.ToString();
            consolidation_max.Text = max.ToString();
            degree1.Visibility = Visibility.Visible;
            degree2.Visibility = Visibility.Visible;
        }

        private void Decimal_Checked(object sender, RoutedEventArgs e)
        {
            isDegrees = false;
            uptrend_range.Text = $"{max + 1} and higher";
            downtrend_range.Text = $"{min - 1} and lower";
            consolidation_min.Text = min.ToString();
            consolidation_max.Text = max.ToString();
            degree1.Visibility = Visibility.Collapsed;
            degree2.Visibility = Visibility.Collapsed;

        }

        private void Consolidation_min_TextChanged(object sender, TextChangedEventArgs e)
        {
            error_alert.Text = "";
            if (string.IsNullOrEmpty(consolidation_min.Text))
            {
                return;
            }
            bool isParseSuccessful = double.TryParse(consolidation_min.Text, out min);
            if (!isParseSuccessful)
            {
                error_alert.Text = "Range limit can only be a number.";
                return;
            }

            if (decimal_option.IsChecked.Value)
            {
                Decimal_Checked(sender, e);
            }
            else if (degrees_option.IsChecked.Value)
            {
                if ((min < -90) || (min > 90))
                {
                    error_alert.Text = "The range must be within -90° and 90°.";
                }
                Degrees_Checked(sender, e);

            }

            if (min == 0)
            {
                error_alert.Text = "The range min cannot equal 0.";
            }
            else if (min == max)
            {
                error_alert.Text = "The range min and max cannot be the same.";

            }
            else if (min > max)
            {
                error_alert.Text = "The range min cannot be greater than the range max.";
            }
        }

        private void Consolidation_max_TextChanged(object sender, TextChangedEventArgs e)
        {
            error_alert.Text = "";
            if (string.IsNullOrEmpty(consolidation_max.Text))
            {
                return;
            }
            bool isParseSuccessful = double.TryParse(consolidation_max.Text, out max);
            if (!isParseSuccessful)
            {
                error_alert.Text = "Range limit can only be a number.";
                return;
            }

            if (decimal_option.IsChecked.Value)
            {
                Decimal_Checked(sender, e);
            }
            else if (degrees_option.IsChecked.Value)
            {
                if((max < -90) || (max > 90))
                {
                    error_alert.Text = "The range must be within -90° and 90°.";
                }
                Degrees_Checked(sender, e);

            }

            if (max == 0)
            {
                error_alert.Text = "The range max cannot equal 0.";
            }
            else if (min == max)
            {
                error_alert.Text = "The range min and max cannot be the same.";

            }
            else if (min > max)
            {
                error_alert.Text = "The range min cannot be greater than the range max.";
            }

        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(error_alert.Text))
            {
                MessageBox.Show("The range set is not valid. Please check the error message or click cancel.");
                return;
            }

            isCancel = false;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            isCancel = true;
            Close();
        }
    }
}
