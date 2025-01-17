using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp4.Models
{
    public partial class MotionPlcData : ObservableObject
    {
        // 门锁状态
        /// <summary>
        /// 进出料门锁1状态
        /// true: 锁定状态
        /// false: 解锁状态
        /// </summary>
        [ObservableProperty]
        private bool _inOutDoor1Lock = false;

        /// <summary>
        /// 进出料门锁2状态
        /// true: 锁定状态
        /// false: 解锁状态
        /// </summary>
        [ObservableProperty]
        private bool _inOutDoor2Lock = false;

        /// <summary>
        /// 维修门锁1状态
        /// true: 锁定状态
        /// false: 解锁状态
        /// </summary>
        [ObservableProperty]
        private bool _maintenanceDoor1Lock = false;

        /// <summary>
        /// 维修门锁2状态
        /// true: 锁定状态
        /// false: 解锁状态
        /// </summary>
        [ObservableProperty]
        private bool _maintenanceDoor2Lock = false;

        /// <summary>
        /// 蜂鸣器状态
        /// true: 开启状态
        /// false: 关闭状态
        /// </summary>
        [ObservableProperty]
        private bool _buzzerStatus = false;

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

        [ObservableProperty]
        private bool _hasCarriage;  // 是否有小车

        [ObservableProperty]
        private bool _carriageHasMaterial;  // 小车区是否有料

        [ObservableProperty]
        private string _paddle1BoatNumber = "";
        [ObservableProperty]
        private string _paddle1Time = "";
        [ObservableProperty]
        private string _paddle1Status = "";

        [ObservableProperty]
        private string _paddle2BoatNumber = "";
        [ObservableProperty]
        private string _paddle2Time = "";
        [ObservableProperty]
        private string _paddle2Status = "";

        [ObservableProperty]
        private string _paddle3BoatNumber = "";
        [ObservableProperty]
        private string _paddle3Time = "";
        [ObservableProperty]
        private string _paddle3Status = "";

        [ObservableProperty]
        private string _paddle4BoatNumber = "";
        [ObservableProperty]
        private string _paddle4Time = "";
        [ObservableProperty]
        private string _paddle4Status = "";

        [ObservableProperty]
        private string _paddle5BoatNumber = "";
        [ObservableProperty]
        private string _paddle5Time = "";
        [ObservableProperty]
        private string _paddle5Status = "";

        [ObservableProperty]
        private string _paddle6BoatNumber = "";
        [ObservableProperty]
        private string _paddle6Time = "";
        [ObservableProperty]
        private string _paddle6Status = "";

        [ObservableProperty]
        private string _storage1BoatNumber = "";
        [ObservableProperty]
        private string _storage1Time = "";
        [ObservableProperty]
        private string _storage1Status = "";

        [ObservableProperty]
        private string _storage2BoatNumber = "";
        [ObservableProperty]
        private string _storage2Time = "";
        [ObservableProperty]
        private string _storage2Status = "";

        [ObservableProperty]
        private string _storage3BoatNumber = "";
        [ObservableProperty]
        private string _storage3Time = "";
        [ObservableProperty]
        private string _storage3Status = "";

        [ObservableProperty]
        private string _storage4BoatNumber = "";
        [ObservableProperty]
        private string _storage4Time = "";
        [ObservableProperty]
        private string _storage4Status = "";

        [ObservableProperty]
        private string _storage5BoatNumber = "";
        [ObservableProperty]
        private string _storage5Time = "";
        [ObservableProperty]
        private string _storage5Status = "";

        [ObservableProperty]
        private string _storage6BoatNumber = "";
        [ObservableProperty]
        private string _storage6Time = "";
        [ObservableProperty]
        private string _storage6Status = "";

        /// <summary>
        /// 水平上轴是否在运动
        /// true: 运动中
        /// false: 停止
        /// </summary>
        [ObservableProperty]
        private bool _horizontal1Moving;

        /// <summary>
        /// 水平下轴是否在运动
        /// true: 运动中
        /// false: 停止
        /// </summary>
        [ObservableProperty]
        private bool _horizontal2Moving;

        /// <summary>
        /// 垂直轴是否在运动
        /// true: 运动中
        /// false: 停止
        /// </summary>
        [ObservableProperty]
        private bool _verticalMoving;
    }
} 