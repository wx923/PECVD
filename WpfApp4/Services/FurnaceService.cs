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

                    // 读取所有数据
                    data.Time = _modbusClient.ReadInt32($"{offset + 0}").Content;
                    data.T1 = _modbusClient.ReadFloat($"{offset + 1}").Content;
                    data.T2 = _modbusClient.ReadFloat($"{offset + 2}").Content;
                    data.T3 = _modbusClient.ReadFloat($"{offset + 3}").Content;
                    data.T4 = _modbusClient.ReadFloat($"{offset + 4}").Content;
                    data.T5 = _modbusClient.ReadFloat($"{offset + 5}").Content;
                    data.T6 = _modbusClient.ReadFloat($"{offset + 6}").Content;
                    data.T7 = _modbusClient.ReadFloat($"{offset + 7}").Content;
                    data.T8 = _modbusClient.ReadFloat($"{offset + 8}").Content;
                    data.T9 = _modbusClient.ReadFloat($"{offset + 9}").Content;
                    data.Sih4 = _modbusClient.ReadFloat($"{offset + 10}").Content;
                    data.N2 = _modbusClient.ReadFloat($"{offset + 11}").Content;
                    data.N2o = _modbusClient.ReadFloat($"{offset + 12}").Content;
                    data.H2 = _modbusClient.ReadFloat($"{offset + 13}").Content;
                    data.Ph3 = _modbusClient.ReadFloat($"{offset + 14}").Content;
                    data.Pressure = _modbusClient.ReadFloat($"{offset + 15}").Content;
                    data.PowerOutput1 = _modbusClient.ReadFloat($"{offset + 16}").Content;
                    data.PowerOutput2 = _modbusClient.ReadFloat($"{offset + 17}").Content;
                    data.ImportExportSpeed = _modbusClient.ReadFloat($"{offset + 18}").Content;
                    data.PlatformSpeed = _modbusClient.ReadFloat($"{offset + 19}").Content;
                    data.UpDownSpeed = _modbusClient.ReadFloat($"{offset + 20}").Content;
                    data.AuxiliaryHeatingTime = _modbusClient.ReadFloat($"{offset + 21}").Content;
                    data.AuxiliaryHeatingTemp = _modbusClient.ReadFloat($"{offset + 22}").Content;
                    data.PulseOn1 = _modbusClient.ReadFloat($"{offset + 23}").Content;
                    data.PulseOff1 = _modbusClient.ReadFloat($"{offset + 24}").Content;
                    data.PulseOn2 = _modbusClient.ReadFloat($"{offset + 25}").Content;
                    data.PulseOff2 = _modbusClient.ReadFloat($"{offset + 26}").Content;
                    data.Current = _modbusClient.ReadFloat($"{offset + 27}").Content;
                    data.Voltage = _modbusClient.ReadFloat($"{offset + 28}").Content;
                    data.PulseFrequency = _modbusClient.ReadFloat($"{offset + 29}").Content;
                    data.PulseVoltage = _modbusClient.ReadFloat($"{offset + 30}").Content;

                    // 在UI线程更新数据
                    int index = furnaceIndex;
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var furnace = Furnaces[index];
                        furnace.Time = data.Time;
                        furnace.T1 = data.T1;
                        furnace.T2 = data.T2;
                        furnace.T3 = data.T3;
                        furnace.T4 = data.T4;
                        furnace.T5 = data.T5;
                        furnace.T6 = data.T6;
                        furnace.T7 = data.T7;
                        furnace.T8 = data.T8;
                        furnace.T9 = data.T9;
                        furnace.Sih4 = data.Sih4;
                        furnace.N2 = data.N2;
                        furnace.N2o = data.N2o;
                        furnace.H2 = data.H2;
                        furnace.Ph3 = data.Ph3;
                        furnace.Pressure = data.Pressure;
                        furnace.PowerOutput1 = data.PowerOutput1;
                        furnace.PowerOutput2 = data.PowerOutput2;
                        furnace.ImportExportSpeed = data.ImportExportSpeed;
                        furnace.PlatformSpeed = data.PlatformSpeed;
                        furnace.UpDownSpeed = data.UpDownSpeed;
                        furnace.AuxiliaryHeatingTime = data.AuxiliaryHeatingTime;
                        furnace.AuxiliaryHeatingTemp = data.AuxiliaryHeatingTemp;
                        furnace.PulseOn1 = data.PulseOn1;
                        furnace.PulseOff1 = data.PulseOff1;
                        furnace.PulseOn2 = data.PulseOn2;
                        furnace.PulseOff2 = data.PulseOff2;
                        furnace.Current = data.Current;
                        furnace.Voltage = data.Voltage;
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