using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp4.Models
{
    public partial class MotionPlcData : ObservableObject
    {
        // 门锁状态
        [ObservableProperty]
        private bool _door1Lock = false;

        [ObservableProperty]
        private bool _door2Lock = false;

        // 炉门气缸状态
        [ObservableProperty]
        private bool _furnaceVerticalCylinder = false;

        [ObservableProperty]
        private bool _furnaceHorizontalCylinder = false;

        // 区域料位状态
        [ObservableProperty]
        private bool _storage1HasMaterial = false;

        [ObservableProperty]
        private bool _storage2HasMaterial = false;

        [ObservableProperty]
        private bool _clampHasMaterial = false;

        // 机械手水平一轴位置状态
        [ObservableProperty]
        private bool _robotHorizontal1ForwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal1BackwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal1OriginLimit = false;

        [ObservableProperty]
        private int _robotHorizontal1UpperLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal1LowerLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal1OriginPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal1CurrentPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal1CurrentSpeed = 1;

        // 机械手水平二轴位置状态
        [ObservableProperty]
        private bool _robotHorizontal2ForwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal2BackwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal2OriginLimit = false;

        [ObservableProperty]
        private int _robotHorizontal2UpperLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal2LowerLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal2OriginPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal2CurrentPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal2CurrentSpeed = 1;

        // 机械手垂直轴位置状态
        [ObservableProperty]
        private bool _robotVerticalUpperLimit = false;

        [ObservableProperty]
        private bool _robotVerticalLowerLimit = false;

        [ObservableProperty]
        private bool _robotVerticalOriginLimit = false;

        [ObservableProperty]
        private int _robotVerticalUpperLimitPosition = 1;

        [ObservableProperty]
        private int _robotVerticalLowerLimitPosition = 1;

        [ObservableProperty]
        private int _robotVerticalOriginPosition = 1;

        [ObservableProperty]
        private int _robotVerticalCurrentPosition = 1;

        [ObservableProperty]
        private int _robotVerticalCurrentSpeed = 1;

        // 桨水平轴位置状态
        [ObservableProperty]
        private bool _clampHorizontalForwardLimit = false;

        [ObservableProperty]
        private bool _clampHorizontalBackwardLimit = false;

        [ObservableProperty]
        private bool _clampHorizontalOriginLimit = false;

        [ObservableProperty]
        private int _clampHorizontalUpperLimit = 1;

        [ObservableProperty]
        private int _clampHorizontalLowerLimit = 1;

        [ObservableProperty]
        private int _clampHorizontalOriginPosition = 1;

        [ObservableProperty]
        private int _clampHorizontalCurrentPosition = 1;

        [ObservableProperty]
        private int _clampHorizontalCurrentSpeed = 1;

        // 桨垂直轴位置状态
        [ObservableProperty]
        private bool _clampVerticalUpperLimit = false;

        [ObservableProperty]
        private bool _clampVerticalLowerLimit = false;

        [ObservableProperty]
        private bool _clampVerticalOriginLimit = false;

        [ObservableProperty]
        private int _clampVerticalUpperLimitPosition = 1;

        [ObservableProperty]
        private int _clampVerticalLowerLimitPosition = 1;

        [ObservableProperty]
        private int _clampVerticalOriginPosition = 1;

        [ObservableProperty]
        private int _clampVerticalCurrentPosition = 1;

        [ObservableProperty]
        private int _clampVerticalCurrentSpeed = 1;

        // 炉内状态
        [ObservableProperty]
        private bool _furnaceStatus = false;
    }
} 