using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp4.Models;
using HslCommunication.ModBus;
using WpfApp4.ViewModel;

namespace WpfApp4.Services
{
    public class FurnaceService
    {
        private static readonly Lazy<FurnaceService> _instance = new Lazy<FurnaceService>(() => new FurnaceService());
        public static FurnaceService Instance => _instance.Value;

        private CancellationTokenSource _cancellationTokenSource;
        private ModbusTcpNet _modbusClient;
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
            _modbusClient = GlobalVM.plcCommunicationService._modbusTcp;
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            Furnaces = new ObservableCollection<FurnaceData>();
            for (int i = 0; i < 6; i++)
            {
                Furnaces.Add(new FurnaceData());
            }

            GlobalVM.SingleObject.OnConnected += StartDataUpdate;
        }

        private Task StartDataUpdate()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(async () => 
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await UpdateFurnaceDataAsync();
                        await Task.Delay(100, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"炉管数据更新异常: {ex.Message}");
                        await Task.Delay(1000, _cancellationTokenSource.Token);
                    }
                }
            }, _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        private async Task UpdateFurnaceDataAsync()
        {
            try
            {
                for (int furnaceIndex = 0; furnaceIndex < Furnaces.Count; furnaceIndex++)
                {
                    var data = new FurnaceData();
                    int offset = furnaceIndex * 100; // 每个炉管数据地址间隔为100

                    // 读取所有测量数据
                    data.MeasuredT1 = _modbusClient.ReadFloat($"{offset + 1}").Content;
                    data.MeasuredT2 = _modbusClient.ReadFloat($"{offset + 2}").Content;
                    data.MeasuredT3 = _modbusClient.ReadFloat($"{offset + 3}").Content;
                    data.MeasuredT4 = _modbusClient.ReadFloat($"{offset + 4}").Content;
                    data.MeasuredT5 = _modbusClient.ReadFloat($"{offset + 5}").Content;
                    data.MeasuredT6 = _modbusClient.ReadFloat($"{offset + 6}").Content;
                    data.MeasuredT7 = _modbusClient.ReadFloat($"{offset + 7}").Content;
                    data.MeasuredT8 = _modbusClient.ReadFloat($"{offset + 8}").Content;
                    data.MeasuredT9 = _modbusClient.ReadFloat($"{offset + 9}").Content;
                    data.MeasuredSiH4 = _modbusClient.ReadFloat($"{offset + 10}").Content;
                    data.MeasuredN2 = _modbusClient.ReadFloat($"{offset + 11}").Content;
                    data.MeasuredN2O = _modbusClient.ReadFloat($"{offset + 12}").Content;
                    data.MeasuredH2 = _modbusClient.ReadFloat($"{offset + 13}").Content;
                    data.MeasuredPh3 = _modbusClient.ReadFloat($"{offset + 14}").Content;
                    data.MeasuredPressure = _modbusClient.ReadFloat($"{offset + 15}").Content;
                    data.MeasuredPower1 = _modbusClient.ReadFloat($"{offset + 16}").Content;
                    data.MeasuredPower2 = _modbusClient.ReadFloat($"{offset + 17}").Content;
                    data.MeasuredCurrent = _modbusClient.ReadFloat($"{offset + 27}").Content;
                    data.MeasuredVoltage = _modbusClient.ReadFloat($"{offset + 28}").Content;
                    data.PulseFrequency = _modbusClient.ReadFloat($"{offset + 29}").Content;
                    data.PulseVoltage = _modbusClient.ReadFloat($"{offset + 30}").Content;

                    // 在UI线程更新数据
                    int index = furnaceIndex;
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var furnace = Furnaces[index];
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Modbus通讯异常: {ex.Message}");
                throw;
            }
        }

        public void Cleanup()
        {
            _cancellationTokenSource?.Cancel();
            GlobalVM.SingleObject.OnConnected -= StartDataUpdate;
        }
    }
} 