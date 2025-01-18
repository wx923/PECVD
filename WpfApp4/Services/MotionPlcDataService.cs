using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp4.Models;
using HslCommunication.ModBus;
using WpfApp.Services;

namespace WpfApp4.Services
{
    public class MotionPlcDataService
    {
        private static readonly Lazy<MotionPlcDataService> _instance = 
            new Lazy<MotionPlcDataService>(() => new MotionPlcDataService());
        public static MotionPlcDataService Instance => _instance.Value;

        private CancellationTokenSource _cancellationTokenSource;
        private ModbusTcpNet _modbusClient;
        private Dispatcher _dispatcher;
        public MotionPlcData MotionPlcData { get; private set; }

        // 添加基础地址属性
        private int BaseAddress { get; set; } = 0;  // 可以根据需要设置初始基础地址

        // 添加上一次状态的记录
        private bool _lastHasCarriage = false;
        private bool _lastCarriageHasMaterial = false;

        // 添加事件
        public event EventHandler CarriageArrivedWithMaterial;    // 小车带料进来 (00->11)
        public event EventHandler MaterialRemovedFromCarriage;    // 料被移走 (11->10)
        public event EventHandler MaterialReturnedToCarriage;     // 料回到小车 (10->11)
        public event EventHandler CarriageLeftWithoutMaterial;    // 小车空车离开 (11->00)

        private MotionPlcDataService()
        {
            MotionPlcData = new MotionPlcData();
            _modbusClient = PlcCommunicationService.Instance.ModbusTcpClients[PlcCommunicationService.PlcType.Motion];
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            // 订阅运动控制PLC的连接状态改变事件
            PlcCommunicationService.Instance.ConnectionStateChanged += OnPlcConnectionStateChanged;
        }

        private void OnPlcConnectionStateChanged(object sender, (PlcCommunicationService.PlcType PlcType, bool IsConnected) e)
        {
            // 只关注运动控制PLC的连接状态
            if (e.PlcType == PlcCommunicationService.PlcType.Motion && e.IsConnected)
            {
                _ = StartDataUpdate();
            }
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
                        await UpdatePlcDataAsync();
                        await Task.Delay(100, _cancellationTokenSource.Token); // 100ms 更新间隔
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"数据更新异常: {ex.Message}");
                        await Task.Delay(1000, _cancellationTokenSource.Token); // 发生异常时等待较长时间
                    }
                }
            }, _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        private void CheckCarriageStateChange(bool currentHasCarriage, bool currentHasMaterial)
        {
            // 获取当前状态组合
            string currentState = $"{(currentHasCarriage ? "1" : "0")}{(currentHasMaterial ? "1" : "0")}";
            string previousState = $"{(_lastHasCarriage ? "1" : "0")}{(_lastCarriageHasMaterial ? "1" : "0")}";

            // 检查状态变化
            if (previousState != currentState)
            {
                switch (previousState + "->" + currentState)
                {
                    case "00->11": // 小车带料进来
                        CarriageArrivedWithMaterial?.Invoke(this, EventArgs.Empty);
                        break;

                    case "11->10": // 料被移走
                        MaterialRemovedFromCarriage?.Invoke(this, EventArgs.Empty);
                        break;

                    case "10->11": // 料回到小车
                        MaterialReturnedToCarriage?.Invoke(this, EventArgs.Empty);
                        break;

                    case "11->00": // 小车空车离开
                        CarriageLeftWithoutMaterial?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }

            // 更新上一次的状态
            _lastHasCarriage = currentHasCarriage;
            _lastCarriageHasMaterial = currentHasMaterial;
        }

        private async Task UpdatePlcDataAsync()
        {
            try
            {
                var data = new MotionPlcData();
                int addr = BaseAddress;  // 从基础地址开始

                // 读取门锁状态
                data.InOutDoor1Lock = _modbusClient.ReadCoil($"{addr++}").Content;              // 地址 base + 0
                data.InOutDoor2Lock = _modbusClient.ReadCoil($"{addr++}").Content;              // 地址 base + 1
                data.MaintenanceDoor1Lock = _modbusClient.ReadCoil($"{addr++}").Content;        // 地址 base + 2
                data.MaintenanceDoor2Lock = _modbusClient.ReadCoil($"{addr++}").Content;        // 地址 base + 3
                data.BuzzerStatus = _modbusClient.ReadCoil($"{addr++}").Content;                // 地址 base + 4
                // 读取机械手位置和速度
                data.RobotHorizontal1CurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;   // 地址 base + 7
                data.RobotHorizontal1CurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;      // 地址 base + 8
                data.RobotHorizontal2CurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;   // 地址 base + 9
                data.RobotHorizontal2CurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;      // 地址 base + 10
                data.RobotVerticalCurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;      // 地址 base + 11
                data.RobotVerticalCurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;         // 地址 base + 12

                // 读取轴运动状态
                data.Horizontal1Moving = _modbusClient.ReadCoil($"{addr++}").Content;          // 地址 base + 13
                data.Horizontal2Moving = _modbusClient.ReadCoil($"{addr++}").Content;          // 地址 base + 14
                data.VerticalMoving = _modbusClient.ReadCoil($"{addr++}").Content;             // 地址 base + 15

                // 读取暂存区舟检测传感器状态
                data.Storage1BoatSensor = _modbusClient.ReadCoil($"{addr++}").Content;         // 地址 base + 16
                data.Storage2BoatSensor = _modbusClient.ReadCoil($"{addr++}").Content;         // 地址 base + 17
                data.Storage3BoatSensor = _modbusClient.ReadCoil($"{addr++}").Content;         // 地址 base + 18
                data.Storage4BoatSensor = _modbusClient.ReadCoil($"{addr++}").Content;         // 地址 base + 19
                data.Storage5BoatSensor = _modbusClient.ReadCoil($"{addr++}").Content;         // 地址 base + 20
                data.Storage6BoatSensor = _modbusClient.ReadCoil($"{addr++}").Content;         // 地址 base + 21

                // 在UI线程更新数据
                await _dispatcher.InvokeAsync(() =>
                {
                    // 更新门锁状态
                    MotionPlcData.InOutDoor1Lock = data.InOutDoor1Lock;
                    MotionPlcData.InOutDoor2Lock = data.InOutDoor2Lock;
                    MotionPlcData.MaintenanceDoor1Lock = data.MaintenanceDoor1Lock;
                    MotionPlcData.MaintenanceDoor2Lock = data.MaintenanceDoor2Lock;
                    MotionPlcData.BuzzerStatus = data.BuzzerStatus;

                    // 更新机械手位置和速度
                    MotionPlcData.RobotHorizontal1CurrentPosition = data.RobotHorizontal1CurrentPosition;
                    MotionPlcData.RobotHorizontal1CurrentSpeed = data.RobotHorizontal1CurrentSpeed;
                    MotionPlcData.RobotHorizontal2CurrentPosition = data.RobotHorizontal2CurrentPosition;
                    MotionPlcData.RobotHorizontal2CurrentSpeed = data.RobotHorizontal2CurrentSpeed;
                    MotionPlcData.RobotVerticalCurrentPosition = data.RobotVerticalCurrentPosition;
                    MotionPlcData.RobotVerticalCurrentSpeed = data.RobotVerticalCurrentSpeed;

                    // 更新轴运动状态
                    MotionPlcData.Horizontal1Moving = data.Horizontal1Moving;
                    MotionPlcData.Horizontal2Moving = data.Horizontal2Moving;
                    MotionPlcData.VerticalMoving = data.VerticalMoving;

                    // 更新暂存区舟检测传感器状态
                    MotionPlcData.Storage1BoatSensor = data.Storage1BoatSensor;
                    MotionPlcData.Storage2BoatSensor = data.Storage2BoatSensor;
                    MotionPlcData.Storage3BoatSensor = data.Storage3BoatSensor;
                    MotionPlcData.Storage4BoatSensor = data.Storage4BoatSensor;
                    MotionPlcData.Storage5BoatSensor = data.Storage5BoatSensor;
                    MotionPlcData.Storage6BoatSensor = data.Storage6BoatSensor;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Modbus通讯异常: {ex.Message}");
                throw;
            }
        }

        // 添加修改基础地址的方法
        public void SetBaseAddress(int newBaseAddress)
        {
            BaseAddress = newBaseAddress;
        }

        public void Cleanup()
        {
            _cancellationTokenSource?.Cancel();
            // 取消订阅事件
            PlcCommunicationService.Instance.ConnectionStateChanged -= OnPlcConnectionStateChanged;
        }
    }
} 