using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp4.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isConnected && isConnected ? "断开" : "连接";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 