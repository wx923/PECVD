using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp4.Converters
{
    public class BoolToSubmitStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSubmitted)
            {
                return isSubmitted ? "已提交" : "未提交";
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 