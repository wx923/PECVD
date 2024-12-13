using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WpfApp4.Models;
using WpfApp4.Services;

namespace WpfApp4.ViewModel
{
    public partial class ProcessMonitoringVM : ObservableObject
    {
        [ObservableProperty]
        private int tubeNumber;

        private readonly int furnaceIndex;  // 用于访问集合的实际索引

        [ObservableProperty]
        private ObservableCollection<FurnaceData> monitoringData;

        public ProcessMonitoringVM(int tubeNumber)
        {
            TubeNumber = tubeNumber;
            furnaceIndex = tubeNumber - 1;  // 将显示用的炉管号转换为实际的集合索引

            // 创建一个只包含当前炉管数据的集合
            MonitoringData = new ObservableCollection<FurnaceData> { FurnaceService.Instance.Furnaces[furnaceIndex] };
        }
    }
} 