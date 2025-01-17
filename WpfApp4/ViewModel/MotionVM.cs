using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HslCommunication.ModBus;
using WpfApp.Services;
using WpfApp4.Models;
using WpfApp4.Services;
using System.Collections.ObjectModel;

namespace WpfApp4.ViewModel
{
    public partial class MotionVM : ObservableObject
    {
        private readonly PlcCommunicationService.PlcType _tubePlcType;  // 炉管PLC类型

        // PLC客户端
        private ModbusTcpNet _robotPlc => PlcCommunicationService.Instance.ModbusTcpClients[PlcCommunicationService.PlcType.Motion];  // 机械手PLC
        private ModbusTcpNet _tubePlc => PlcCommunicationService.Instance.ModbusTcpClients[_tubePlcType];  // 炉管PLC
        private MotionPlcDataService _plcDataService => MotionPlcDataService.Instance;
        
        // 获取PLC数据的属性
        public MotionPlcData MotionPlcData => _plcDataService.MotionPlcData;

        [ObservableProperty]
        private int tubeNumber;

        // 事件日志相关
        public class EventLog
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }

        [ObservableProperty]
        private ObservableCollection<EventLog> _eventLogs = new ObservableCollection<EventLog>();

        // 按钮启用状态属性
        [ObservableProperty]
        private bool _isPauseEnabled;

        [ObservableProperty]
        private bool _isResumeEnabled;

        [ObservableProperty]
        private bool _isStepEnabled;

        [ObservableProperty]
        private bool _isStartEnabled = true;  // 启动按钮状态

        public MotionVM(int tubeNumber)
        {
            TubeNumber = tubeNumber;
            
            // 根据炉管号选择对应的PLC类型
            _tubePlcType = tubeNumber switch
            {
                1 => PlcCommunicationService.PlcType.Furnace1,
                2 => PlcCommunicationService.PlcType.Furnace2,
                3 => PlcCommunicationService.PlcType.Furnace3,
                4 => PlcCommunicationService.PlcType.Furnace4,
                5 => PlcCommunicationService.PlcType.Furnace5,
                6 => PlcCommunicationService.PlcType.Furnace6,
                _ => throw new ArgumentException($"无效的炉管号: {tubeNumber}")
            };
            
            EventLogs = new ObservableCollection<EventLog>();
            EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"初始化炉管 {tubeNumber} 的运动控制" });
        }

        // 源位置和目标位置
        [ObservableProperty]
        private string _selectedSourcePosition;

        [ObservableProperty]
        private string _selectedTargetPosition;

        // 模式选择状态
        [ObservableProperty]
        private bool _isAutoModeSelected = false;

        [ObservableProperty]
        private bool _isJogModeSelected = false;

        [ObservableProperty]
        private bool _isValueModeSelected = false;

        // 模式切换命令
        [RelayCommand]
        private void AutoMode()
        {
            IsAutoModeSelected = true;
            IsJogModeSelected = false;
            IsValueModeSelected = false;
            
            // 重置按钮状态
            IsStartEnabled = true;
            IsPauseEnabled = false;
            IsResumeEnabled = false;
            IsStepEnabled = false;
        }

        [RelayCommand]
        private void JogMode()
        {
            IsAutoModeSelected = false;
            IsJogModeSelected = true;
            IsValueModeSelected = false;
            
            // 重置按钮状态
            IsStartEnabled = false;
            IsPauseEnabled = false;
            IsResumeEnabled = false;
            IsStepEnabled = false;
        }

        [RelayCommand]
        private void ValueMode()
        {
            IsAutoModeSelected = false;
            IsJogModeSelected = false;
            IsValueModeSelected = true;
            
            // 重置按钮状态
            IsStartEnabled = false;
            IsPauseEnabled = false;
            IsResumeEnabled = false;
            IsStepEnabled = false;
        }

        // 获取位置代码
        private byte GetPositionCode(string position)
        {
            return position switch
            {
                "小车区" => 1,
                "暂存区1" => 2,
                "暂存区2" => 3,
                "暂存区3" => 4,
                "暂存区4" => 5,
                "暂存区5" => 6,
                "暂存区6" => 7,
                "桨区1" => 8,
                "桨区2" => 9,
                "桨区3" => 10,
                "桨区4" => 11,
                "桨区5" => 12,
                "桨区6" => 13,
                _ => throw new ArgumentException($"未知的位置: {position}")
            };
        }

        // 自动模式启动命令
        [RelayCommand]
        private async Task AutoModeStart()
        {
            try
            {
                // 检查是否选择了源位置和目标位置
                if (string.IsNullOrEmpty(SelectedSourcePosition))
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "请选择源位置" });
                    return;
                }

                if (string.IsNullOrEmpty(SelectedTargetPosition))
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "请选择目标位置" });
                    return;
                }

                // 检查源位置和目标位置是否相同
                if (SelectedSourcePosition == SelectedTargetPosition)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "源位置和目标位置不能相同" });
                    return;
                }

                // 检查轴运动状态
                if (MotionPlcData.Horizontal1Moving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "水平上轴正在运动中，无法启动" });
                    return;
                }
                
                if (MotionPlcData.Horizontal2Moving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "水平下轴正在运动中，无法启动" });
                    return;
                }
                
                if (MotionPlcData.VerticalMoving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "垂直轴正在运动中，无法启动" });
                    return;
                }

                // 获取源位置和目标位置的代号
                byte sourceCode = GetPositionCode(SelectedSourcePosition);
                byte targetCode = GetPositionCode(SelectedTargetPosition);
                
                // 如果所有轴都没有在运动，则继续执行启动逻辑
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"开始执行自动运动: 从 {SelectedSourcePosition} 到 {SelectedTargetPosition}" });
                
                // 更新按钮状态
                IsStartEnabled = false;  // 禁用启动按钮
                IsPauseEnabled = true;   // 启用暂停按钮
                IsResumeEnabled = false; // 禁用恢复按钮
                IsStepEnabled = false;   // 禁用步进按钮
                
                // 写入机械手PLC
                await _robotPlc.WriteAsync("443", sourceCode);  // 写入源位置代号
                await _robotPlc.WriteAsync("444", targetCode);  // 写入目标位置代号
                await _robotPlc.WriteAsync("442", true);  // 写入启动信号
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"启动失败: {ex.Message}" });
            }
        }

        // 暂停命令
        [RelayCommand]
        private async Task Pause()
        {
            try
            {
                await _robotPlc.WriteAsync("459", true);  // 写入暂停信号
                
                // 更新按钮状态
                IsPauseEnabled = false;  // 禁用暂停按钮
                IsResumeEnabled = true;  // 启用恢复按钮
                IsStepEnabled = true;    // 启用步进按钮
                
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "自动运动已暂停" });
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"暂停失败: {ex.Message}" });
            }
        }

        // 恢复命令
        [RelayCommand]
        private async Task Resume()
        {
            try
            {
                await _robotPlc.WriteAsync("460", true);  // 写入恢复信号
                
                // 更新按钮状态
                IsPauseEnabled = true;   // 启用暂停按钮
                IsResumeEnabled = false; // 禁用恢复按钮
                IsStepEnabled = false;   // 禁用步进按钮
                
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "自动运动已恢复" });
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"恢复失败: {ex.Message}" });
            }
        }

        // 步进命令
        [RelayCommand]
        private async Task NextStep()
        {
            try
            {
                await _robotPlc.WriteAsync("461", true);  // 写入下一步信号
                
                // 更新按钮状态
                IsPauseEnabled = true;   // 启用暂停按钮
                IsResumeEnabled = false; // 禁用恢复按钮
                IsStepEnabled = false;   // 禁用步进按钮
                
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "执行下一步" });
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"下一步失败: {ex.Message}" });
            }
        }

        [RelayCommand]
        private async Task PreviousStep()
        {
            try
            {
                await _robotPlc.WriteAsync("462", true);  // 写入上一步信号
                
                // 更新按钮状态
                IsPauseEnabled = true;   // 启用暂停按钮
                IsResumeEnabled = false; // 禁用恢复按钮
                IsStepEnabled = false;   // 禁用步进按钮
                
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "执行上一步" });
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"上一步失败: {ex.Message}" });
            }
        }

        // 蜂鸣器控制命令
        [RelayCommand]
        private async Task ToggleBuzzer()
        {
            try
            {
                if (MotionPlcData.BuzzerStatus)
                {
                    await _robotPlc.WriteAsync("4", false);  // 发送关闭命令
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"蜂鸣器控制失败: {ex.Message}");
            }
        }
    }
}
