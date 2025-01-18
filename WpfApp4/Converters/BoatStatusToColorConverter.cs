using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp4.Converters
{
    public class BoatStatusToColorConverter : IValueConverter
    {
        private string GetStatusText(int status)
        {
            return status switch
            {
                1 => "未工艺舟",
                2 => "已工艺舟",
                3 => "工艺中舟",
                4 => "工艺失败舟",
                5 => "冷却中舟",
                6 => "冷却完成舟",
                _ => string.Empty
            };
        }

        private SolidColorBrush GetStatusColor(int status)
        {
            return status switch
            {
                1 => new SolidColorBrush(Colors.Gray),
                2 => new SolidColorBrush(Colors.Green),
                3 => new SolidColorBrush(Colors.Yellow),
                4 => new SolidColorBrush(Colors.Red),
                5 => new SolidColorBrush(Colors.LightBlue),
                6 => new SolidColorBrush(Colors.DarkBlue),
                _ => new SolidColorBrush(Colors.Transparent)
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                // 如果目标类型是Brush，返回颜色
                if (targetType == typeof(Brush))
                {
                    return GetStatusColor(status);
                }
                // 否则返回状态文本
                return GetStatusText(status);
            }
            return targetType == typeof(Brush) ? new SolidColorBrush(Colors.Transparent) : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 