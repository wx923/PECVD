using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp4.Models
{
    public partial class FurnaceData : ObservableObject
    {
        [ObservableProperty]
        private int _time;  // 时间

        [ObservableProperty]
        private float _t1;  // T1

        [ObservableProperty]
        private float _t2;  // T2

        [ObservableProperty]
        private float _t3;  // T3

        [ObservableProperty]
        private float _t4;  // T4

        [ObservableProperty]
        private float _t5;  // T5

        [ObservableProperty]
        private float _t6;  // T6

        [ObservableProperty]
        private float _t7;  // T7

        [ObservableProperty]
        private float _t8;  // T8

        [ObservableProperty]
        private float _t9;  // T9

        [ObservableProperty]
        private float _sih4;  // SiH4

        [ObservableProperty]
        private float _n2;  // N2

        [ObservableProperty]
        private float _n2o;  // N2O

        [ObservableProperty]
        private float _h2;   // H2

        [ObservableProperty]
        private float _ph3;  // PH3

        [ObservableProperty]
        private float _pressure;  // 压力

        [ObservableProperty]
        private float _powerOutput1;  // 功率1

        [ObservableProperty]
        private float _powerOutput2;  // 功率2

        [ObservableProperty]
        private float _importExportSpeed;  // 进/出舟移动速度

        [ObservableProperty]
        private float _platformSpeed;  // 平移速度

        [ObservableProperty]
        private float _upDownSpeed;  // 上下速度

        [ObservableProperty]
        private float _auxiliaryHeatingTime;  // 辅热时间

        [ObservableProperty]
        private float _auxiliaryHeatingTemp;  // 辅热温度

        [ObservableProperty]
        private float _pulseOn1;  // 脉冲开1

        [ObservableProperty]
        private float _pulseOff1;  // 脉冲关1

        [ObservableProperty]
        private float _pulseOn2;  // 脉冲开2

        [ObservableProperty]
        private float _pulseOff2;  // 脉冲关2

        [ObservableProperty]
        private float _current;  // 电流

        [ObservableProperty]
        private float _voltage;  // 电压

        [ObservableProperty]
        private float _pulseFrequency;  // 射频电流

        [ObservableProperty]
        private float _pulseVoltage;  // 射频电压
    }
} 