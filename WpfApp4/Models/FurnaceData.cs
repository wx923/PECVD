using CommunityToolkit.Mvvm.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WpfApp4.Models
{
    public partial class FurnaceData : ProcessExcelModel
    {
        [ObservableProperty]
        private string processCollectionName = string.Empty;

        /// <summary>
        /// 脉冲频率实测值
        /// </summary>
        [ObservableProperty]
        private float pulseFrequency;

        /// <summary>
        /// 脉冲电压实测值
        /// </summary>
        [ObservableProperty]
        private float pulseVoltage;

        public bool IsProcessRunning { get; set; }
    }
}