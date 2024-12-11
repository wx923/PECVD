using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp4.Models;
using HslCommunication.ModBus;
using WpfApp4.ViewModel;

namespace WpfApp4.Services
{
    public class PlcDataService
    {
        private static readonly Lazy<PlcDataService> _instance = new Lazy<PlcDataService>(() => new PlcDataService());
        public static PlcDataService Instance => _instance.Value;

        private CancellationTokenSource _cancellationTokenSource;
        private ModbusTcpNet _modbusClient;
        private Dispatcher _dispatcher;
        public MotionPlcData MotionPlcData { get; private set; }

        private PlcDataService()
        {
            MotionPlcData = new MotionPlcData();
            _modbusClient = GlobalVM.plcCommunicationService._modbusTcp;
            _dispatcher = Dispatcher.CurrentDispatcher;
            
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

        private async Task UpdatePlcDataAsync()
        {
            try
            {
                // 在后台线程读取数据
                var data = new MotionPlcData();
                
                // 读取线圈状态
                data.Door1Lock = _modbusClient.ReadCoil("1").Content;
                data.Door2Lock = _modbusClient.ReadCoil("2").Content;
                data.FurnaceVerticalCylinder = _modbusClient.ReadCoil("3").Content;
                data.FurnaceHorizontalCylinder = _modbusClient.ReadCoil("4").Content;
                data.Storage1HasMaterial = _modbusClient.ReadCoil("5").Content;
                data.Storage2HasMaterial = _modbusClient.ReadCoil("6").Content;
                data.ClampHasMaterial = _modbusClient.ReadCoil("7").Content;

                // 机械手水平一轴
                data.RobotHorizontal1ForwardLimit = _modbusClient.ReadCoil("8").Content;
                data.RobotHorizontal1BackwardLimit = _modbusClient.ReadCoil("9").Content;
                data.RobotHorizontal1OriginLimit = _modbusClient.ReadCoil("10").Content;
                data.RobotHorizontal1UpperLimit = _modbusClient.ReadInt32("11").Content;
                data.RobotHorizontal1LowerLimit = _modbusClient.ReadInt32("12").Content;
                data.RobotHorizontal1OriginPosition = _modbusClient.ReadInt32("13").Content;
                data.RobotHorizontal1CurrentPosition = _modbusClient.ReadInt32("14").Content;
                data.RobotHorizontal1CurrentSpeed = _modbusClient.ReadInt32("15").Content;

                // 机械手水平二轴
                data.RobotHorizontal2ForwardLimit = _modbusClient.ReadCoil("16").Content;
                data.RobotHorizontal2BackwardLimit = _modbusClient.ReadCoil("17").Content;
                data.RobotHorizontal2OriginLimit = _modbusClient.ReadCoil("18").Content;
                data.RobotHorizontal2UpperLimit = _modbusClient.ReadInt32("19").Content;
                data.RobotHorizontal2LowerLimit = _modbusClient.ReadInt32("20").Content;
                data.RobotHorizontal2OriginPosition = _modbusClient.ReadInt32("21").Content;
                data.RobotHorizontal2CurrentPosition = _modbusClient.ReadInt32("22").Content;
                data.RobotHorizontal2CurrentSpeed = _modbusClient.ReadInt32("23").Content;

                // 机械手垂直轴
                data.RobotVerticalUpperLimit = _modbusClient.ReadCoil("24").Content;
                data.RobotVerticalLowerLimit = _modbusClient.ReadCoil("25").Content;
                data.RobotVerticalOriginLimit = _modbusClient.ReadCoil("26").Content;
                data.RobotVerticalUpperLimitPosition = _modbusClient.ReadInt32("27").Content;
                data.RobotVerticalLowerLimitPosition = _modbusClient.ReadInt32("28").Content;
                data.RobotVerticalOriginPosition = _modbusClient.ReadInt32("29").Content;
                data.RobotVerticalCurrentPosition = _modbusClient.ReadInt32("30").Content;
                data.RobotVerticalCurrentSpeed = _modbusClient.ReadInt32("31").Content;

                // 桨水平轴
                data.ClampHorizontalForwardLimit = _modbusClient.ReadCoil("32").Content;
                data.ClampHorizontalBackwardLimit = _modbusClient.ReadCoil("33").Content;
                data.ClampHorizontalOriginLimit = _modbusClient.ReadCoil("34").Content;
                data.ClampHorizontalUpperLimit = _modbusClient.ReadInt32("35").Content;
                data.ClampHorizontalLowerLimit = _modbusClient.ReadInt32("36").Content;
                data.ClampHorizontalOriginPosition = _modbusClient.ReadInt32("37").Content;
                data.ClampHorizontalCurrentPosition = _modbusClient.ReadInt32("38").Content;
                data.ClampHorizontalCurrentSpeed = _modbusClient.ReadInt32("39").Content;

                // 桨垂直轴
                data.ClampVerticalUpperLimit = _modbusClient.ReadCoil("40").Content;
                data.ClampVerticalLowerLimit = _modbusClient.ReadCoil("41").Content;
                data.ClampVerticalOriginLimit = _modbusClient.ReadCoil("42").Content;
                data.ClampVerticalUpperLimitPosition = _modbusClient.ReadInt32("43").Content;
                data.ClampVerticalLowerLimitPosition = _modbusClient.ReadInt32("44").Content;
                data.ClampVerticalOriginPosition = _modbusClient.ReadInt32("45").Content;
                data.ClampVerticalCurrentPosition = _modbusClient.ReadInt32("46").Content;
                data.ClampVerticalCurrentSpeed = _modbusClient.ReadInt32("47").Content;

                // 炉内状态
                data.FurnaceStatus = _modbusClient.ReadCoil("48").Content;

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
                });
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