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

        // 区域料位状态
        [ObservableProperty]
        private bool _hasCarriage;  // 是否有小车

        [ObservableProperty]
        private bool _carriageHasMaterial;  // 小车区是否有料

        // 机械手水平一轴位置状态
        [ObservableProperty]
        private int _robotHorizontal1CurrentPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal1CurrentSpeed = 1;

        // 机械手水平二轴位置状态
        [ObservableProperty]
        private int _robotHorizontal2CurrentPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal2CurrentSpeed = 1;

        // 机械手垂直轴位置状态
        [ObservableProperty]
        private int _robotVerticalCurrentPosition = 1;

        [ObservableProperty]
        private int _robotVerticalCurrentSpeed = 1;

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

        // 暂存区舟检测传感器状态
        /// <summary>
        /// 暂存区1舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _storage1BoatSensor = false;

        /// <summary>
        /// 暂存区2舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _storage2BoatSensor = false;

        /// <summary>
        /// 暂存区3舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _storage3BoatSensor = false;

        /// <summary>
        /// 暂存区4舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _storage4BoatSensor = false;

        /// <summary>
        /// 暂存区5舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _storage5BoatSensor = false;

        /// <summary>
        /// 暂存区6舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _storage6BoatSensor = false;
    }
} 