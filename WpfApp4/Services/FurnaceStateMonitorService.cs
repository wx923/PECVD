using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp4.Models;
using HslCommunication.ModBus;
using WpfApp.Services;

namespace WpfApp4.Services
{
    public class FurnaceStateMonitorService
    {
        private static readonly Lazy<FurnaceStateMonitorService> _instance = 
            new Lazy<FurnaceStateMonitorService>(() => new FurnaceStateMonitorService());
        public static FurnaceStateMonitorService Instance => _instance.Value;

        private Dictionary<int, ModbusTcpNet> _modbusClients;
        private Dictionary<int, bool> _previousProcessStates;
        private Dispatcher _dispatcher;

        // 修改事件定义，使用基础的 EventHandler
        public event EventHandler ProcessStarted;
        public event EventHandler ProcessEnded;

        private FurnaceStateMonitorService()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _modbusClients = new Dictionary<int, ModbusTcpNet>();
            _previousProcessStates = new Dictionary<int, bool>();

            // 初始化ModbusTcp客户端和状态字典
            for (int i = 0; i < 6; i++)
            {
                _modbusClients[i] = PlcCommunicationService.Instance.ModbusTcpClients[(PlcCommunicationService.PlcType)i];
                _previousProcessStates[i] = false;
            }
        }

        /// <summary>
        /// 获取指定炉管的工艺运行状态
        /// </summary>
        /// <param name="furnaceIndex">炉管索引</param>
        /// <returns>工艺运行状态，true表示正在运行，false表示未运行</returns>
        public async Task<bool> GetProcessRunningStateAsync(int furnaceIndex)
        {
            try
            {
                if (!_modbusClients.TryGetValue(furnaceIndex, out var modbusClient))
                {
                    throw new Exception($"找不到炉管{furnaceIndex}的PLC连接");
                }

                int offset = furnaceIndex * 100;
                // 读取工艺运行状态线圈（假设使用地址60作为工艺运行状态）
                var result = await Task.Run(() => modbusClient.ReadCoil($"{offset + 60}"));
                if (!result.IsSuccess)
                {
                    throw new Exception($"读取炉管{furnaceIndex}工艺状态失败: {result.Message}");
                }

                bool currentState = result.Content;
                bool previousState = _previousProcessStates[furnaceIndex];

                // 检测状态变化并触发相应事件
                if (currentState != previousState)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        if (currentState) // 从false变为true，工艺开始
                        {
                            ProcessStarted?.Invoke(this, EventArgs.Empty);
                        }
                        else // 从true变为false，工艺结束
                        {
                            ProcessEnded?.Invoke(this, EventArgs.Empty);
                        }
                    });

                    _previousProcessStates[furnaceIndex] = currentState;
                }

                return currentState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取炉管{furnaceIndex}工艺状态失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 清空所有事件订阅
        /// </summary>
        public void ClearEvents()
        {
            ProcessStarted = null;
            ProcessEnded = null;
        }
    }
} 