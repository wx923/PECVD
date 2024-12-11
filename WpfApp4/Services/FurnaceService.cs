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

        // 使用ObservableCollection来存储炉管数据
        public ObservableCollection<FurnaceData> Furnaces { get; private set; }

        // 当前选中的炉管索引
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

        // 获取当前选中的炉管数据
        public FurnaceData CurrentFurnace => Furnaces[SelectedFurnaceIndex];

        private FurnaceService()
        {
            _modbusClient = GlobalVM.plcCommunicationService._modbusTcp;
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            // 初始化6个炉管
            Furnaces = new ObservableCollection<FurnaceData>();
            for (int i = 0; i < 6; i++)
            {
                Furnaces.Add(new FurnaceData());
            }

            // 订阅连接成功事件
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
                        await Task.Delay(100, _cancellationTokenSource.Token); // 100ms 更新间隔
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
                // 为每个炉管读取数据
                for (int furnaceIndex = 0; furnaceIndex < Furnaces.Count; furnaceIndex++)
                {
                    var data = new FurnaceData();
                    int offset = furnaceIndex * 100; // 假设每个炉管的数据地址间隔为100

                    // 读取数据
                    data.Time = _modbusClient.ReadInt32($"{offset + 1}").Content;
                    data.T1 = _modbusClient.ReadInt32($"{offset + 2}").Content;
                    data.T2 = _modbusClient.ReadInt32($"{offset + 3}").Content;
                    // ... 读取其他数据 ...

                    // 在UI线程更新数据
                    int index = furnaceIndex; // 捕获循环变量
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var furnace = Furnaces[index];
                        furnace.Time = data.Time;
                        furnace.T1 = data.T1;
                        furnace.T2 = data.T2;
                        // ... 更新其他属性 ...
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