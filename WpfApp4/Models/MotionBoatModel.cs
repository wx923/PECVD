using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp4.Models
{
    public partial class MotionBoatModel : ObservableObject
    {
        /// <summary>
        /// 舟编号
        /// </summary>
        [ObservableProperty]
        public int _boatNumber;

        /// <summary>
        /// 舟所在位置
        /// 1: 小车区
        /// 2-7: 暂存区1-6
        /// 8-13: 桨区1-6
        /// </summary>
        [ObservableProperty]
        public int _location;

        /// <summary>
        /// 舟的状态
        /// 1: 未工艺舟 - 灰色
        /// 2: 已工艺舟 - 绿色
        /// 3: 工艺中舟 - 黄色
        /// 4: 工艺失败舟 - 红色
        /// 5: 冷却中舟 - 浅蓝色
        /// 6: 冷却完成舟 - 深蓝色
        /// </summary>
        [ObservableProperty]
        public int _status;

        /// <summary>
        /// 当前冷却时间(分钟)
        /// </summary>
        [ObservableProperty]
        public int _currentCoolingTime;

        /// <summary>
        /// 总冷却时间(分钟)
        /// </summary>
        [ObservableProperty]
        public int _totalCoolingTime;
    }

    /// <summary>
    /// 区域舟信息(用于展示桨区和暂存区)
    /// </summary>
    public partial class AreaBoatInfo : ObservableObject
    {
        /// <summary>
        /// 舟编号
        /// </summary>
        [ObservableProperty]
        public int _boatNumber;

        /// <summary>
        /// 当前冷却时间(分钟)
        /// </summary>
        [ObservableProperty]
        public int _currentCoolingTime;

        /// <summary>
        /// 总冷却时间(分钟)
        /// </summary>
        [ObservableProperty]
        public int _totalCoolingTime;

        /// <summary>
        /// 舟的状态
        /// 1: 未工艺舟 - 灰色
        /// 2: 已工艺舟 - 绿色
        /// 3: 工艺中舟 - 黄色
        /// 4: 工艺失败舟 - 红色
        /// 5: 冷却中舟 - 浅蓝色
        /// 6: 冷却完成舟 - 深蓝色
        /// </summary>
        [ObservableProperty]
        public int _status;
    }
} 