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

                // 读取所有测量数据并映射到ProcessExcelModel的属性
                data.T1 = (int)modbusClient.ReadFloat($"{offset + 1}").Content;
                data.T2 = (int)modbusClient.ReadFloat($"{offset + 2}").Content;
                data.T3 = (int)modbusClient.ReadFloat($"{offset + 3}").Content;
                data.T4 = (int)modbusClient.ReadFloat($"{offset + 4}").Content;
                data.T5 = (int)modbusClient.ReadFloat($"{offset + 5}").Content;
                data.T6 = (int)modbusClient.ReadFloat($"{offset + 6}").Content;
                data.T7 = (int)modbusClient.ReadFloat($"{offset + 7}").Content;
                data.T8 = (int)modbusClient.ReadFloat($"{offset + 8}").Content;
                data.T9 = (int)modbusClient.ReadFloat($"{offset + 9}").Content;
                data.SiH4 = (int)modbusClient.ReadFloat($"{offset + 10}").Content;
                data.N2 = (int)modbusClient.ReadFloat($"{offset + 11}").Content;
                data.N2O = (int)modbusClient.ReadFloat($"{offset + 12}").Content;
                data.H2 = (int)modbusClient.ReadFloat($"{offset + 13}").Content;
                data.Ph3 = (int)modbusClient.ReadFloat($"{offset + 14}").Content;
                data.Pressure = (int)modbusClient.ReadFloat($"{offset + 15}").Content;
                data.Power1 = (int)modbusClient.ReadFloat($"{offset + 16}").Content;
                data.Power2 = (int)modbusClient.ReadFloat($"{offset + 17}").Content;
                data.CurrentReference = (int)modbusClient.ReadFloat($"{offset + 27}").Content;
                data.VoltageReference = (int)modbusClient.ReadFloat($"{offset + 28}").Content;
                data.PulseFrequency = modbusClient.ReadFloat($"{offset + 29}").Content;
                data.PulseVoltage = modbusClient.ReadFloat($"{offset + 30}").Content;

                // 在UI线程更新数据
                await _dispatcher.InvokeAsync(() =>
                {
                    var furnace = Furnaces[furnaceIndex];
                    furnace.T1 = data.T1;
                    furnace.T2 = data.T2;
                    furnace.T3 = data.T3;
                    furnace.T4 = data.T4;
                    furnace.T5 = data.T5;
                    furnace.T6 = data.T6;
                    furnace.T7 = data.T7;
                    furnace.T8 = data.T8;
                    furnace.T9 = data.T9;
                    furnace.SiH4 = data.SiH4;
                    furnace.N2 = data.N2;
                    furnace.N2O = data.N2O;
                    furnace.H2 = data.H2;
                    furnace.Ph3 = data.Ph3;
                    furnace.Pressure = data.Pressure;
                    furnace.Power1 = data.Power1;
                    furnace.Power2 = data.Power2;
                    furnace.CurrentReference = data.CurrentReference;
                    furnace.VoltageReference = data.VoltageReference;
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