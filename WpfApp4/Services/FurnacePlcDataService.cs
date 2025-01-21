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
    public class FurnacePlcDataService
    {
        private static readonly Lazy<FurnacePlcDataService> _instance = 
            new Lazy<FurnacePlcDataService>(() => new FurnacePlcDataService());
        public static FurnacePlcDataService Instance => _instance.Value;

        private CancellationTokenSource _cancellationTokenSource;
        private Dictionary<int, ModbusTcpNet> _modbusClients;
        private Dispatcher _dispatcher;

        // 每个炉管的PLC数据
        public Dictionary<int, FurnacePlcData> FurnacePlcDataDict { get; private set; }

        private FurnacePlcDataService()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _modbusClients = new Dictionary<int, ModbusTcpNet>();
            FurnacePlcDataDict = new Dictionary<int, FurnacePlcData>();

            // 初始化6个炉管的PLC客户端和数据对象
            for (int i = 0; i < 6; i++)
            {
                _modbusClients[i] = PlcCommunicationService.Instance.ModbusTcpClients[(PlcCommunicationService.PlcType)i];
                FurnacePlcDataDict[i] = new FurnacePlcData();
            }

            // 订阅每个炉管PLC的连接状态改变事件
            PlcCommunicationService.Instance.ConnectionStateChanged += OnPlcConnectionStateChanged;
        }

        private void OnPlcConnectionStateChanged(object sender, (PlcCommunicationService.PlcType PlcType, bool IsConnected) e)
        {
            // 只处理炉管PLC的连接状态（PlcType 0-5）
            if ((int)e.PlcType < 6 && e.IsConnected)
            {
                _ = StartDataUpdate((int)e.PlcType);
            }
        }

        private Task StartDataUpdate(int furnaceIndex)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await UpdatePlcDataAsync(furnaceIndex);
                        await Task.Delay(100, _cancellationTokenSource.Token); // 100ms 更新间隔
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"炉管{furnaceIndex + 1} 数据更新异常: {ex.Message}");
                        await Task.Delay(1000, _cancellationTokenSource.Token); // 发生异常时等待较长时间
                    }
                }
            }, _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        private async Task UpdatePlcDataAsync(int furnaceIndex)
        {
            try
            {
                var data = new FurnacePlcData();
                int addr = 0;  // 从基础地址开始

                // 读取桨区舟检测传感器状态
                data.PaddleBoatSensor = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;    // 地址 0

                // 读取炉门气缸状态
                data.VerticalFurnaceDoorCylinder = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;      // 地址 1
                data.HorizontalFurnaceDoorCylinder = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;    // 地址 2

                // 读取轴运动状态
                data.HorizontalAxisMoving = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;    // 地址 3
                data.VerticalAxisMoving = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;      // 地址 4

                // 读取水平轴限位传感器状态
                data.HorizontalUpperLimit = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;    // 地址 5
                data.HorizontalOriginLimit = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;   // 地址 6
                data.HorizontalLowerLimit = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;    // 地址 7

                // 读取垂直轴限位传感器状态
                data.VerticalUpperLimit = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;      // 地址 8
                data.VerticalOriginLimit = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;     // 地址 9
                data.VerticalLowerLimit = _modbusClients[furnaceIndex].ReadCoil($"{addr++}").Content;      // 地址 10

                // 在UI线程更新数据
                await _dispatcher.InvokeAsync(() =>
                {
                    // 更新桨区舟检测传感器状态
                    FurnacePlcDataDict[furnaceIndex].PaddleBoatSensor = data.PaddleBoatSensor;

                    // 更新炉门气缸状态
                    FurnacePlcDataDict[furnaceIndex].VerticalFurnaceDoorCylinder = data.VerticalFurnaceDoorCylinder;
                    FurnacePlcDataDict[furnaceIndex].HorizontalFurnaceDoorCylinder = data.HorizontalFurnaceDoorCylinder;

                    // 更新轴运动状态
                    FurnacePlcDataDict[furnaceIndex].HorizontalAxisMoving = data.HorizontalAxisMoving;
                    FurnacePlcDataDict[furnaceIndex].VerticalAxisMoving = data.VerticalAxisMoving;

                    // 更新水平轴限位传感器状态
                    FurnacePlcDataDict[furnaceIndex].HorizontalUpperLimit = data.HorizontalUpperLimit;
                    FurnacePlcDataDict[furnaceIndex].HorizontalOriginLimit = data.HorizontalOriginLimit;
                    FurnacePlcDataDict[furnaceIndex].HorizontalLowerLimit = data.HorizontalLowerLimit;

                    // 更新垂直轴限位传感器状态
                    FurnacePlcDataDict[furnaceIndex].VerticalUpperLimit = data.VerticalUpperLimit;
                    FurnacePlcDataDict[furnaceIndex].VerticalOriginLimit = data.VerticalOriginLimit;
                    FurnacePlcDataDict[furnaceIndex].VerticalLowerLimit = data.VerticalLowerLimit;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"炉管{furnaceIndex + 1} Modbus通讯异常: {ex.Message}");
                throw;
            }
        }

        public void Cleanup()
        {
            _cancellationTokenSource?.Cancel();
            PlcCommunicationService.Instance.ConnectionStateChanged -= OnPlcConnectionStateChanged;
        }
    }
} 