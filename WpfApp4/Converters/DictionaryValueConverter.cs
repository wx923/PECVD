using System;
using System.Globalization;
using System.Windows.Data;
using WpfApp4.Models;

namespace WpfApp4.Converters
{
    public class DictionaryValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProcessExcelModel data && parameter is string key)
            {
                return key switch
                {
                    "N2O" => data.N2O,
                    "H2" => data.H2,
                    "PH3" => data.Ph3,
                    "压力" => data.Pressure,
                    "功率1" => data.Power1,
                    "功率2" => data.Power2,
                    "进/出舟" => data.BoatInOut,
                    "平移速度" => data.MoveSpeed,
                    "上下速度" => data.UpDownSpeed,
                    "辅热时间" => data.HeatTime,
                    "辅热温度" => data.HeatTemp,
                    "脉冲开1" => data.PulseOn1,
                    "脉冲关1" => data.PulseOff1,
                    "脉冲开2" => data.PulseOn2,
                    "脉冲关2" => data.PulseOff2,
                    "电流电压" => data.CurrentVoltage,
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 