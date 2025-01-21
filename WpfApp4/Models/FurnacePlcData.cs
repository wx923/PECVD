using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp4.Models
{
    public partial class FurnacePlcData : ObservableObject
    {
        // 桨区舟检测传感器状态
        /// <summary>
        /// 桨区舟检测传感器状态
        /// true: 有舟
        /// false: 无舟
        /// </summary>
        [ObservableProperty]
        private bool _paddleBoatSensor = false;

        // 炉门气缸状态
        /// <summary>
        /// 垂直炉门气缸状态
        /// true: 打开状态
        /// false: 关闭状态
        /// </summary>
        [ObservableProperty]
        private bool _verticalFurnaceDoorCylinder = false;

        /// <summary>
        /// 水平炉门气缸状态
        /// true: 打开状态
        /// false: 关闭状态
        /// </summary>
        [ObservableProperty]
        private bool _horizontalFurnaceDoorCylinder = false;

        /// <summary>
        /// 水平轴运动状态
        /// true: 正在运动
        /// false: 静止状态
        /// </summary>
        [ObservableProperty]
        private bool _horizontalAxisMoving = false;

        /// <summary>
        /// 垂直轴运动状态
        /// true: 正在运动
        /// false: 静止状态
        /// </summary>
        [ObservableProperty]
        private bool _verticalAxisMoving = false;

        // 水平轴限位传感器
        /// <summary>
        /// 水平轴上限位传感器
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _horizontalUpperLimit = false;

        /// <summary>
        /// 水平轴原点传感器
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _horizontalOriginLimit = false;

        /// <summary>
        /// 水平轴下限位传感器
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _horizontalLowerLimit = false;

        // 垂直轴限位传感器
        /// <summary>
        /// 垂直轴上限位传感器
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _verticalUpperLimit = false;

        /// <summary>
        /// 垂直轴原点传感器
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _verticalOriginLimit = false;

        /// <summary>
        /// 垂直轴下限位传感器
        /// true: 触发
        /// false: 未触发
        /// </summary>
        [ObservableProperty]
        private bool _verticalLowerLimit = false;

        /// <summary>
        /// 水平轴当前位置（单位：mm）
        /// </summary>
        [ObservableProperty]
        private double _horizontalPosition = 0;

        /// <summary>
        /// 垂直轴当前位置（单位：mm）
        /// </summary>
        [ObservableProperty]
        private double _verticalPosition = 0;

        public FurnacePlcData()
        {
            // 初始化所有属性为默认值
            PaddleBoatSensor = false;
            VerticalFurnaceDoorCylinder = false;
            HorizontalFurnaceDoorCylinder = false;
            HorizontalAxisMoving = false;
            VerticalAxisMoving = false;
            HorizontalUpperLimit = false;
            HorizontalLowerLimit = false;
            HorizontalOriginLimit = false;
            VerticalUpperLimit = false;
            VerticalLowerLimit = false;
            VerticalOriginLimit = false;
            HorizontalPosition = 0;
            VerticalPosition = 0;
        }
    }
} 