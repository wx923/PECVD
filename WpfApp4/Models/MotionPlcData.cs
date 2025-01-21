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

        /// <summary>
        /// 水平上轴前限位
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotHorizontal1ForwardLimit;

        /// <summary>
        /// 水平上轴后限位
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotHorizontal1BackwardLimit;

        /// <summary>
        /// 水平上轴原点
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotHorizontal1OriginLimit;

        /// <summary>
        /// 水平下轴前限位
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotHorizontal2ForwardLimit;

        /// <summary>
        /// 水平下轴后限位
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotHorizontal2BackwardLimit;

        /// <summary>
        /// 水平下轴原点
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotHorizontal2OriginLimit;

        /// <summary>
        /// 垂直轴上限位
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotVerticalUpperLimit;

        /// <summary>
        /// 垂直轴下限位
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotVerticalLowerLimit;

        /// <summary>
        /// 垂直轴原点
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _robotVerticalOriginLimit;

        /// <summary>
        /// 水平上轴当前位置（单位：mm）
        /// </summary>
        [ObservableProperty]
        private double _robotHorizontal1Position;

        /// <summary>
        /// 水平下轴当前位置（单位：mm）
        /// </summary>
        [ObservableProperty]
        private double _robotHorizontal2Position;

        /// <summary>
        /// 垂直轴当前位置（单位：mm）
        /// </summary>
        [ObservableProperty]
        private double _robotVerticalPosition;

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