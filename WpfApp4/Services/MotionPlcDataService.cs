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

                // 读取线圈状态
                data.Door1Lock = _modbusClient.ReadCoil($"{addr++}").Content;              // 地址 base + 0
                data.Door2Lock = _modbusClient.ReadCoil($"{addr++}").Content;              // 地址 base + 1
                data.FurnaceVerticalCylinder = _modbusClient.ReadCoil($"{addr++}").Content;    // 地址 base + 2
                data.FurnaceHorizontalCylinder = _modbusClient.ReadCoil($"{addr++}").Content;  // 地址 base + 3
                data.Storage1HasMaterial = _modbusClient.ReadCoil($"{addr++}").Content;        // 地址 base + 4
                data.Storage2HasMaterial = _modbusClient.ReadCoil($"{addr++}").Content;        // 地址 base + 5
                data.ClampHasMaterial = _modbusClient.ReadCoil($"{addr++}").Content;           // 地址 base + 6
                data.HasCarriage = _modbusClient.ReadCoil($"{addr++}").Content;                // 地址 base + 7
                data.CarriageHasMaterial = _modbusClient.ReadCoil($"{addr++}").Content;        // 地址 base + 8

                // 机械手水平一轴
                data.RobotHorizontal1ForwardLimit = _modbusClient.ReadCoil($"{addr++}").Content;   // 地址 base + 9
                data.RobotHorizontal1BackwardLimit = _modbusClient.ReadCoil($"{addr++}").Content;  // 地址 base + 10
                data.RobotHorizontal1OriginLimit = _modbusClient.ReadCoil($"{addr++}").Content;    // 地址 base + 11
                data.RobotHorizontal1UpperLimit = _modbusClient.ReadInt32($"{addr++}").Content;    // 地址 base + 12
                data.RobotHorizontal1LowerLimit = _modbusClient.ReadInt32($"{addr++}").Content;    // 地址 base + 13
                data.RobotHorizontal1OriginPosition = _modbusClient.ReadInt32($"{addr++}").Content;    // 地址 base + 14
                data.RobotHorizontal1CurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;   // 地址 base + 15
                data.RobotHorizontal1CurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;      // 地址 base + 16

                // 机械手水平二轴
                data.RobotHorizontal2ForwardLimit = _modbusClient.ReadCoil($"{addr++}").Content;   // 地址 base + 17
                data.RobotHorizontal2BackwardLimit = _modbusClient.ReadCoil($"{addr++}").Content;  // 地址 base + 18
                data.RobotHorizontal2OriginLimit = _modbusClient.ReadCoil($"{addr++}").Content;    // 地址 base + 19
                data.RobotHorizontal2UpperLimit = _modbusClient.ReadInt32($"{addr++}").Content;    // 地址 base + 20
                data.RobotHorizontal2LowerLimit = _modbusClient.ReadInt32($"{addr++}").Content;    // 地址 base + 21
                data.RobotHorizontal2OriginPosition = _modbusClient.ReadInt32($"{addr++}").Content;    // 地址 base + 22
                data.RobotHorizontal2CurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;   // 地址 base + 23
                data.RobotHorizontal2CurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;      // 地址 base + 24

                // 机械手垂直轴
                data.RobotVerticalUpperLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.RobotVerticalLowerLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.RobotVerticalOriginLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.RobotVerticalUpperLimitPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.RobotVerticalLowerLimitPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.RobotVerticalOriginPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.RobotVerticalCurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.RobotVerticalCurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;

                // 桨水平轴
                data.ClampHorizontalForwardLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.ClampHorizontalBackwardLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.ClampHorizontalOriginLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.ClampHorizontalUpperLimit = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampHorizontalLowerLimit = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampHorizontalOriginPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampHorizontalCurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampHorizontalCurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;

                // 桨垂直轴
                data.ClampVerticalUpperLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.ClampVerticalLowerLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.ClampVerticalOriginLimit = _modbusClient.ReadCoil($"{addr++}").Content;
                data.ClampVerticalUpperLimitPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampVerticalLowerLimitPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampVerticalOriginPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampVerticalCurrentPosition = _modbusClient.ReadInt32($"{addr++}").Content;
                data.ClampVerticalCurrentSpeed = _modbusClient.ReadInt32($"{addr++}").Content;

                // 炉内状态
                data.FurnaceStatus = _modbusClient.ReadCoil($"{addr++}").Content;          // 最后一个地址

                // 在UI线程更新数据
                await _dispatcher.InvokeAsync(() =>
                {
                    // 更新所有属性
                    MotionPlcData.Door1Lock = data.Door1Lock;
                    MotionPlcData.Door2Lock = data.Door2Lock;
                    MotionPlcData.FurnaceVerticalCylinder = data.FurnaceVerticalCylinder;
                    MotionPlcData.FurnaceHorizontalCylinder = data.FurnaceHorizontalCylinder;
                    MotionPlcData.Storage1HasMaterial = data.Storage1HasMaterial;
                    MotionPlcData.Storage2HasMaterial = data.Storage2HasMaterial;
                    MotionPlcData.ClampHasMaterial = data.ClampHasMaterial;

                    // 机械手水平一轴
                    MotionPlcData.RobotHorizontal1ForwardLimit = data.RobotHorizontal1ForwardLimit;
                    MotionPlcData.RobotHorizontal1BackwardLimit = data.RobotHorizontal1BackwardLimit;
                    MotionPlcData.RobotHorizontal1OriginLimit = data.RobotHorizontal1OriginLimit;
                    MotionPlcData.RobotHorizontal1UpperLimit = data.RobotHorizontal1UpperLimit;
                    MotionPlcData.RobotHorizontal1LowerLimit = data.RobotHorizontal1LowerLimit;
                    MotionPlcData.RobotHorizontal1OriginPosition = data.RobotHorizontal1OriginPosition;
                    MotionPlcData.RobotHorizontal1CurrentPosition = data.RobotHorizontal1CurrentPosition;
                    MotionPlcData.RobotHorizontal1CurrentSpeed = data.RobotHorizontal1CurrentSpeed;

                    // 机械手水平二轴
                    MotionPlcData.RobotHorizontal2ForwardLimit = data.RobotHorizontal2ForwardLimit;
                    MotionPlcData.RobotHorizontal2BackwardLimit = data.RobotHorizontal2BackwardLimit;
                    MotionPlcData.RobotHorizontal2OriginLimit = data.RobotHorizontal2OriginLimit;
                    MotionPlcData.RobotHorizontal2UpperLimit = data.RobotHorizontal2UpperLimit;
                    MotionPlcData.RobotHorizontal2LowerLimit = data.RobotHorizontal2LowerLimit;
                    MotionPlcData.RobotHorizontal2OriginPosition = data.RobotHorizontal2OriginPosition;
                    MotionPlcData.RobotHorizontal2CurrentPosition = data.RobotHorizontal2CurrentPosition;
                    MotionPlcData.RobotHorizontal2CurrentSpeed = data.RobotHorizontal2CurrentSpeed;

                    // 机械手垂直轴
                    MotionPlcData.RobotVerticalUpperLimit = data.RobotVerticalUpperLimit;
                    MotionPlcData.RobotVerticalLowerLimit = data.RobotVerticalLowerLimit;
                    MotionPlcData.RobotVerticalOriginLimit = data.RobotVerticalOriginLimit;
                    MotionPlcData.RobotVerticalUpperLimitPosition = data.RobotVerticalUpperLimitPosition;
                    MotionPlcData.RobotVerticalLowerLimitPosition = data.RobotVerticalLowerLimitPosition;
                    MotionPlcData.RobotVerticalOriginPosition = data.RobotVerticalOriginPosition;
                    MotionPlcData.RobotVerticalCurrentPosition = data.RobotVerticalCurrentPosition;
                    MotionPlcData.RobotVerticalCurrentSpeed = data.RobotVerticalCurrentSpeed;

                    // 桨水平轴
                    MotionPlcData.ClampHorizontalForwardLimit = data.ClampHorizontalForwardLimit;
                    MotionPlcData.ClampHorizontalBackwardLimit = data.ClampHorizontalBackwardLimit;
                    MotionPlcData.ClampHorizontalOriginLimit = data.ClampHorizontalOriginLimit;
                    MotionPlcData.ClampHorizontalUpperLimit = data.ClampHorizontalUpperLimit;
                    MotionPlcData.ClampHorizontalLowerLimit = data.ClampHorizontalLowerLimit;
                    MotionPlcData.ClampHorizontalOriginPosition = data.ClampHorizontalOriginPosition;
                    MotionPlcData.ClampHorizontalCurrentPosition = data.ClampHorizontalCurrentPosition;
                    MotionPlcData.ClampHorizontalCurrentSpeed = data.ClampHorizontalCurrentSpeed;

                    // 桨垂直轴
                    MotionPlcData.ClampVerticalUpperLimit = data.ClampVerticalUpperLimit;
                    MotionPlcData.ClampVerticalLowerLimit = data.ClampVerticalLowerLimit;
                    MotionPlcData.ClampVerticalOriginLimit = data.ClampVerticalOriginLimit;
                    MotionPlcData.ClampVerticalUpperLimitPosition = data.ClampVerticalUpperLimitPosition;
                    MotionPlcData.ClampVerticalLowerLimitPosition = data.ClampVerticalLowerLimitPosition;
                    MotionPlcData.ClampVerticalOriginPosition = data.ClampVerticalOriginPosition;
                    MotionPlcData.ClampVerticalCurrentPosition = data.ClampVerticalCurrentPosition;
                    MotionPlcData.ClampVerticalCurrentSpeed = data.ClampVerticalCurrentSpeed;

                    // 炉内状态
                    MotionPlcData.FurnaceStatus = data.FurnaceStatus;

                    // 更新新属性并检查状态变化
                    MotionPlcData.HasCarriage = data.HasCarriage;
                    MotionPlcData.CarriageHasMaterial = data.CarriageHasMaterial;
                    
                    // 检查状态变化
                    CheckCarriageStateChange(data.HasCarriage, data.CarriageHasMaterial);
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