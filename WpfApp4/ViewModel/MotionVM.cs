﻿using System;
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
        // PLC客户端
        private ModbusTcpNet _robotPlc => PlcCommunicationService.Instance.ModbusTcpClients[PlcCommunicationService.PlcType.Motion];  // 机械手PLC
        private MotionPlcDataService _plcDataService => MotionPlcDataService.Instance;
        private FurnacePlcDataService _furnacePlcDataService => FurnacePlcDataService.Instance;
        private MotionBoatService _boatService => MotionBoatService.Instance;
        
        // 获取PLC数据的属性
        public MotionPlcData MotionPlcData => _plcDataService.MotionPlcData;
        public Dictionary<int, FurnacePlcData> FurnacePlcDataDict => _furnacePlcDataService.FurnacePlcDataDict;

        // 区域舟信息
        [ObservableProperty]
        private ObservableCollection<AreaBoatInfo> _storageAreas;

        [ObservableProperty]
        private ObservableCollection<AreaBoatInfo> _paddleAreas;

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

        public MotionVM()
        {
            // 初始化区域舟信息集合
            StorageAreas = new ObservableCollection<AreaBoatInfo>();
            PaddleAreas = new ObservableCollection<AreaBoatInfo>();

            // 初始化6个暂存区和6个桨区
            for (int i = 0; i < 6; i++)
            {
                StorageAreas.Add(new AreaBoatInfo());
                PaddleAreas.Add(new AreaBoatInfo());
            }

            // 启动区域舟信息更新
            StartAreaBoatInfoUpdate();
            
            EventLogs = new ObservableCollection<EventLog>();
            EventLogs.Add(new EventLog { Time = DateTime.Now, Message = "初始化运动控制" });
        }

        private void StartAreaBoatInfoUpdate()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var boats = _boatService.Boats;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // 清空所有区域的舟信息
                            foreach (var area in StorageAreas)
                            {
                                area.BoatNumber = 0;
                                area.CurrentCoolingTime = 0;
                                area.TotalCoolingTime = 0;
                                area.Status = 2;
                            }
                            foreach (var area in PaddleAreas)
                            {
                                area.BoatNumber = 0;
                                area.CurrentCoolingTime = 0;
                                area.TotalCoolingTime = 0;
                                area.Status = 4;
                            }

                            // 更新区域舟信息
                            foreach (var boat in boats)
                            {
                                if (boat.Location >= 1 && boat.Location <= 6)  // 暂存区
                                {
                                    var area = StorageAreas[boat.Location - 1];
                                    UpdateAreaInfo(area, boat);
                                }
                                else if (boat.Location >= 7 && boat.Location <= 12)  // 桨区
                                {
                                    var area = PaddleAreas[boat.Location - 7];
                                    UpdateAreaInfo(area, boat);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"更新区域舟信息失败: {ex.Message}" });
                    }

                    await Task.Delay(1000); // 每秒更新一次
                }
            });
        }

        private void UpdateAreaInfo(AreaBoatInfo area, MotionBoatModel boat)
        {
            area.BoatNumber = boat.BoatNumber;
            area.CurrentCoolingTime = boat.CurrentCoolingTime;
            area.TotalCoolingTime = boat.TotalCoolingTime;
            area.Status = boat.Status;
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
                "暂存区1" => 1,
                "暂存区2" => 2,
                "暂存区3" => 3,
                "暂存区4" => 4,
                "暂存区5" => 5,
                "暂存区6" => 6,
                "桨区1" => 7,
                "桨区2" => 8,
                "桨区3" => 9,
                "桨区4" => 10,
                "桨区5" => 11,
                "桨区6" => 12,
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

        // 进入炉内命令
        [RelayCommand]
        private async Task MoveIntoFurnace(string furnaceNumber)
        {
            try
            {
                // 将炉管编号转换为索引（0-5）
                int furnaceIndex = int.Parse(furnaceNumber) - 1;
                
                // 获取对应炉管的PLC数据
                var furnacePlcData = _furnacePlcDataService.FurnacePlcDataDict[furnaceIndex];

                // 检查水平轴和垂直轴是否在运动
                if (furnacePlcData.HorizontalAxisMoving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}水平轴正在运动中，无法进入炉内" });
                    return;
                }

                if (furnacePlcData.VerticalAxisMoving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}垂直轴正在运动中，无法进入炉内" });
                    return;
                }

                // 获取对应炉管的PLC客户端
                var tubePlc = PlcCommunicationService.Instance.ModbusTcpClients[(PlcCommunicationService.PlcType)furnaceIndex];

                // 发送进入炉内命令到PLC
                await tubePlc.WriteAsync("100", true);  // 假设地址100为进入炉内命令
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}开始进入炉内" });
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}进入炉内失败: {ex.Message}" });
            }
        }

        // 移动出炉命令
        [RelayCommand]
        private async Task MoveOutFurnace(string furnaceNumber)
        {
            try
            {
                // 将炉管编号转换为索引（0-5）
                int furnaceIndex = int.Parse(furnaceNumber) - 1;
                
                // 获取对应炉管的PLC数据
                var furnacePlcData = _furnacePlcDataService.FurnacePlcDataDict[furnaceIndex];

                // 检查水平轴和垂直轴是否在运动
                if (furnacePlcData.HorizontalAxisMoving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}水平轴正在运动中，无法移动出炉" });
                    return;
                }

                if (furnacePlcData.VerticalAxisMoving)
                {
                    EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}垂直轴正在运动中，无法移动出炉" });
                    return;
                }

                // 获取对应炉管的PLC客户端
                var tubePlc = PlcCommunicationService.Instance.ModbusTcpClients[(PlcCommunicationService.PlcType)furnaceIndex];

                // 发送移动出炉命令到PLC
                await tubePlc.WriteAsync("101", true);  // 假设地址101为移动出炉命令
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}开始移动出炉" });
            }
            catch (Exception ex)
            {
                EventLogs.Add(new EventLog { Time = DateTime.Now, Message = $"炉管{furnaceNumber}移动出炉失败: {ex.Message}" });
            }
        }
    }
}
