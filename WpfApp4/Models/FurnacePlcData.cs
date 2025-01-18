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
    }
} 