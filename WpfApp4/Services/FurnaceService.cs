using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp4.Models;
using HslCommunication.ModBus;
using WpfApp.Services;
using System.Collections.Generic;

namespace WpfApp4.Services
{
    public class FurnaceService
    {
        private static readonly Lazy<FurnaceService> _instance = new Lazy<FurnaceService>(() => new FurnaceService());
        public static FurnaceService Instance => _instance.Value;

        private CancellationTokenSource _cancellationTokenSource;
        private Dictionary<int, ModbusTcpNet> _modbusClients;
        private Dispatcher _dispatcher;

        public ObservableCollection<FurnaceData> Furnaces { get; private set; }

        private int _selectedFurnaceIndex = 0;
        public int SelectedFurnaceIndex
        {
            get => _selectedFurnaceIndex;
            set
            {
                if (value >= 0 && value < Furnaces.Count)
                {
                    _selectedFurnaceIndex = value;
                }
            }
        }

        public FurnaceData CurrentFurnace => Furnaces[SelectedFurnaceIndex];

        private FurnaceService()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            // 初始化炉管数据集合
            Furnaces = new ObservableCollection<FurnaceData>();
            for (int i = 0; i < 6; i++)
            {
                Furnaces.Add(new FurnaceData());
            }

            // 获取所有炉管的ModbusTcp客户端
            _modbusClients = new Dictionary<int, ModbusTcpNet>();
            for (int i = 0; i < 6; i++)
            {
                _modbusClients[i] = PlcCommunicationService.Instance.ModbusTcpClients[(PlcCommunicationService.PlcType)i];
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
            // 确保有效的炉管索引
            if (furnaceIndex < 0 || furnaceIndex >= 6)
            {
                return Task.CompletedTask;
            }

            // 如果已经有该炉管的更新任务在运行，先取消它
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(async () => 
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await UpdateFurnaceDataAsync(furnaceIndex);
                        await Task.Delay(100, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"炉管{furnaceIndex}数据更新异常: {ex.Message}");
                        await Task.Delay(1000, _cancellationTokenSource.Token);
                    }
                }
            }, _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        private async Task UpdateFurnaceDataAsync(int furnaceIndex)
        {
            try
            {
                var data = new FurnaceData();
                int offset = furnaceIndex * 100;
                var modbusClient = _modbusClients[furnaceIndex];

                // 读取所有测量数据
                data.MeasuredT1 = modbusClient.ReadFloat($"{offset + 1}").Content;
                data.MeasuredT2 = modbusClient.ReadFloat($"{offset + 2}").Content;
                data.MeasuredT3 = modbusClient.ReadFloat($"{offset + 3}").Content;
                data.MeasuredT4 = modbusClient.ReadFloat($"{offset + 4}").Content;
                data.MeasuredT5 = modbusClient.ReadFloat($"{offset + 5}").Content;
                data.MeasuredT6 = modbusClient.ReadFloat($"{offset + 6}").Content;
                data.MeasuredT7 = modbusClient.ReadFloat($"{offset + 7}").Content;
                data.MeasuredT8 = modbusClient.ReadFloat($"{offset + 8}").Content;
                data.MeasuredT9 = modbusClient.ReadFloat($"{offset + 9}").Content;
                data.MeasuredSiH4 = modbusClient.ReadFloat($"{offset + 10}").Content;
                data.MeasuredN2 = modbusClient.ReadFloat($"{offset + 11}").Content;
                data.MeasuredN2O = modbusClient.ReadFloat($"{offset + 12}").Content;
                data.MeasuredH2 = modbusClient.ReadFloat($"{offset + 13}").Content;
                data.MeasuredPh3 = modbusClient.ReadFloat($"{offset + 14}").Content;
                data.MeasuredPressure = modbusClient.ReadFloat($"{offset + 15}").Content;
                data.MeasuredPower1 = modbusClient.ReadFloat($"{offset + 16}").Content;
                data.MeasuredPower2 = modbusClient.ReadFloat($"{offset + 17}").Content;
                data.MeasuredCurrent = modbusClient.ReadFloat($"{offset + 27}").Content;
                data.MeasuredVoltage = modbusClient.ReadFloat($"{offset + 28}").Content;
                data.PulseFrequency = modbusClient.ReadFloat($"{offset + 29}").Content;
                data.PulseVoltage = modbusClient.ReadFloat($"{offset + 30}").Content;

                // 在UI线程更新数据
                await _dispatcher.InvokeAsync(() =>
                {
                    var furnace = Furnaces[furnaceIndex];
                    furnace.MeasuredT1 = data.MeasuredT1;
                    furnace.MeasuredT2 = data.MeasuredT2;
                    furnace.MeasuredT3 = data.MeasuredT3;
                    furnace.MeasuredT4 = data.MeasuredT4;
                    furnace.MeasuredT5 = data.MeasuredT5;
                    furnace.MeasuredT6 = data.MeasuredT6;
                    furnace.MeasuredT7 = data.MeasuredT7;
                    furnace.MeasuredT8 = data.MeasuredT8;
                    furnace.MeasuredT9 = data.MeasuredT9;
                    furnace.MeasuredSiH4 = data.MeasuredSiH4;
                    furnace.MeasuredN2 = data.MeasuredN2;
                    furnace.MeasuredN2O = data.MeasuredN2O;
                    furnace.MeasuredH2 = data.MeasuredH2;
                    furnace.MeasuredPh3 = data.MeasuredPh3;
                    furnace.MeasuredPressure = data.MeasuredPressure;
                    furnace.MeasuredPower1 = data.MeasuredPower1;
                    furnace.MeasuredPower2 = data.MeasuredPower2;
                    furnace.MeasuredCurrent = data.MeasuredCurrent;
                    furnace.MeasuredVoltage = data.MeasuredVoltage;
                    furnace.PulseFrequency = data.PulseFrequency;
                    furnace.PulseVoltage = data.PulseVoltage;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"炉管{furnaceIndex} Modbus通讯异常: {ex.Message}");
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