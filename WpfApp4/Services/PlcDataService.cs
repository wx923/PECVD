using System;
using System.Windows.Threading;
using WpfApp4.Models;
using HslCommunication.ModBus;
using WpfApp4.ViewModel;
using System.Threading.Tasks;

namespace WpfApp4.Services
{
    public class PlcDataService
    {
        private static readonly Lazy<PlcDataService> _instance = new Lazy<PlcDataService>(() => new PlcDataService());
        public static PlcDataService Instance => _instance.Value;

        private DispatcherTimer _dataUpdateTimer;
        private ModbusTcpNet _modbusClient;
        public MotionPlcData MotionPlcData { get; private set; }

        private PlcDataService()
        {
            MotionPlcData = new MotionPlcData();
            _modbusClient = GlobalVM.plcCommunicationService._modbusTcp;
            
            _dataUpdateTimer = new DispatcherTimer();
            _dataUpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _dataUpdateTimer.Tick += UpdatePlcData;

            // 订阅连接成功事件
            GlobalVM.SingleObject.OnConnected += StartDataUpdate;
        }

        private Task StartDataUpdate()
        {
            _dataUpdateTimer.Start();
            return Task.CompletedTask;
        }

        private void UpdatePlcData(object sender, EventArgs e)
        {
            try
            {
                // 读取线圈状态
                MotionPlcData.Door1Lock = _modbusClient.ReadCoil("1").Content;
                MotionPlcData.Door2Lock = _modbusClient.ReadCoil("2").Content;
                MotionPlcData.FurnaceVerticalCylinder = _modbusClient.ReadCoil("3").Content;
                MotionPlcData.FurnaceHorizontalCylinder = _modbusClient.ReadCoil("4").Content;
                MotionPlcData.Storage1HasMaterial = _modbusClient.ReadCoil("5").Content;
                MotionPlcData.Storage2HasMaterial = _modbusClient.ReadCoil("6").Content;
                MotionPlcData.ClampHasMaterial = _modbusClient.ReadCoil("7").Content;

                // 机械手水平一轴
                MotionPlcData.RobotHorizontal1ForwardLimit = _modbusClient.ReadCoil("8").Content;
                MotionPlcData.RobotHorizontal1BackwardLimit = _modbusClient.ReadCoil("9").Content;
                MotionPlcData.RobotHorizontal1OriginLimit = _modbusClient.ReadCoil("10").Content;
                MotionPlcData.RobotHorizontal1UpperLimit = _modbusClient.ReadInt32("11").Content;
                MotionPlcData.RobotHorizontal1LowerLimit = _modbusClient.ReadInt32("12").Content;
                MotionPlcData.RobotHorizontal1OriginPosition = _modbusClient.ReadInt32("13").Content;
                MotionPlcData.RobotHorizontal1CurrentPosition = _modbusClient.ReadInt32("14").Content;
                MotionPlcData.RobotHorizontal1CurrentSpeed = _modbusClient.ReadInt32("15").Content;

                // 机械手水平二轴
                MotionPlcData.RobotHorizontal2ForwardLimit = _modbusClient.ReadCoil("16").Content;
                MotionPlcData.RobotHorizontal2BackwardLimit = _modbusClient.ReadCoil("17").Content;
                MotionPlcData.RobotHorizontal2OriginLimit = _modbusClient.ReadCoil("18").Content;
                MotionPlcData.RobotHorizontal2UpperLimit = _modbusClient.ReadInt32("19").Content;
                MotionPlcData.RobotHorizontal2LowerLimit = _modbusClient.ReadInt32("20").Content;
                MotionPlcData.RobotHorizontal2OriginPosition = _modbusClient.ReadInt32("21").Content;
                MotionPlcData.RobotHorizontal2CurrentPosition = _modbusClient.ReadInt32("22").Content;
                MotionPlcData.RobotHorizontal2CurrentSpeed = _modbusClient.ReadInt32("23").Content;

                // 机械手垂直轴
                MotionPlcData.RobotVerticalUpperLimit = _modbusClient.ReadCoil("24").Content;
                MotionPlcData.RobotVerticalLowerLimit = _modbusClient.ReadCoil("25").Content;
                MotionPlcData.RobotVerticalOriginLimit = _modbusClient.ReadCoil("26").Content;
                MotionPlcData.RobotVerticalUpperLimitPosition = _modbusClient.ReadInt32("27").Content;
                MotionPlcData.RobotVerticalLowerLimitPosition = _modbusClient.ReadInt32("28").Content;
                MotionPlcData.RobotVerticalOriginPosition = _modbusClient.ReadInt32("29").Content;
                MotionPlcData.RobotVerticalCurrentPosition = _modbusClient.ReadInt32("30").Content;
                MotionPlcData.RobotVerticalCurrentSpeed = _modbusClient.ReadInt32("31").Content;

                // 桨水平轴
                MotionPlcData.ClampHorizontalForwardLimit = _modbusClient.ReadCoil("32").Content;
                MotionPlcData.ClampHorizontalBackwardLimit = _modbusClient.ReadCoil("33").Content;
                MotionPlcData.ClampHorizontalOriginLimit = _modbusClient.ReadCoil("34").Content;
                MotionPlcData.ClampHorizontalUpperLimit = _modbusClient.ReadInt32("35").Content;
                MotionPlcData.ClampHorizontalLowerLimit = _modbusClient.ReadInt32("36").Content;
                MotionPlcData.ClampHorizontalOriginPosition = _modbusClient.ReadInt32("37").Content;
                MotionPlcData.ClampHorizontalCurrentPosition = _modbusClient.ReadInt32("38").Content;
                MotionPlcData.ClampHorizontalCurrentSpeed = _modbusClient.ReadInt32("39").Content;

                // 桨垂直轴
                MotionPlcData.ClampVerticalUpperLimit = _modbusClient.ReadCoil("40").Content;
                MotionPlcData.ClampVerticalLowerLimit = _modbusClient.ReadCoil("41").Content;
                MotionPlcData.ClampVerticalOriginLimit = _modbusClient.ReadCoil("42").Content;
                MotionPlcData.ClampVerticalUpperLimitPosition = _modbusClient.ReadInt32("43").Content;
                MotionPlcData.ClampVerticalLowerLimitPosition = _modbusClient.ReadInt32("44").Content;
                MotionPlcData.ClampVerticalOriginPosition = _modbusClient.ReadInt32("45").Content;
                MotionPlcData.ClampVerticalCurrentPosition = _modbusClient.ReadInt32("46").Content;
                MotionPlcData.ClampVerticalCurrentSpeed = _modbusClient.ReadInt32("47").Content;

                // 炉内状态
                MotionPlcData.FurnaceStatus = _modbusClient.ReadCoil("48").Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Modbus通讯异常: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            _dataUpdateTimer.Stop();
            GlobalVM.SingleObject.OnConnected -= StartDataUpdate;
        }
    }
} 