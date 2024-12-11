using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HslCommunication.ModBus;
using System.Windows.Input;

namespace WpfApp4.ViewModel
{
    public partial class MotionPlcData : ObservableObject
    {
        // 门锁状态
        [ObservableProperty]
        private bool _door1Lock = false;

        [ObservableProperty]
        private bool _door2Lock = false;

        // 炉门气缸状态
        [ObservableProperty]
        private bool _furnaceVerticalCylinder = false;

        [ObservableProperty]
        private bool _furnaceHorizontalCylinder = false;

        // 区域料位状态
        [ObservableProperty]
        private bool _storage1HasMaterial = false;

        [ObservableProperty]
        private bool _storage2HasMaterial = false;

        [ObservableProperty]
        private bool _clampHasMaterial = false;

        // 机械手水平一轴位置状态
        [ObservableProperty]
        private bool _robotHorizontal1ForwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal1BackwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal1OriginLimit = false;

        [ObservableProperty]
        private int _robotHorizontal1UpperLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal1LowerLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal1OriginPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal1CurrentPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal1CurrentSpeed = 1;

        // 机械手水平二轴位置状态
        [ObservableProperty]
        private bool _robotHorizontal2ForwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal2BackwardLimit = false;

        [ObservableProperty]
        private bool _robotHorizontal2OriginLimit = false;

        [ObservableProperty]
        private int _robotHorizontal2UpperLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal2LowerLimit = 1;

        [ObservableProperty]
        private int _robotHorizontal2OriginPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal2CurrentPosition = 1;

        [ObservableProperty]
        private int _robotHorizontal2CurrentSpeed = 1;

        // 机械手垂直轴位置状态
        [ObservableProperty]
        private bool _robotVerticalUpperLimit = false;

        [ObservableProperty]
        private bool _robotVerticalLowerLimit = false;

        [ObservableProperty]
        private bool _robotVerticalOriginLimit = false;

        [ObservableProperty]
        private int _robotVerticalUpperLimitPosition = 1;

        [ObservableProperty]
        private int _robotVerticalLowerLimitPosition = 1;

        [ObservableProperty]
        private int _robotVerticalOriginPosition = 1;

        [ObservableProperty]
        private int _robotVerticalCurrentPosition = 1;

        [ObservableProperty]
        private int _robotVerticalCurrentSpeed = 1;

        // 桨水平轴位置状态
        [ObservableProperty]
        private bool _clampHorizontalForwardLimit = false;

        [ObservableProperty]
        private bool _clampHorizontalBackwardLimit = false;

        [ObservableProperty]
        private bool _clampHorizontalOriginLimit = false;

        [ObservableProperty]
        private int _clampHorizontalUpperLimit = 1;

        [ObservableProperty]
        private int _clampHorizontalLowerLimit = 1;

        [ObservableProperty]
        private int _clampHorizontalOriginPosition = 1;

        [ObservableProperty]
        private int _clampHorizontalCurrentPosition = 1;

        [ObservableProperty]
        private int _clampHorizontalCurrentSpeed = 1;

        // 桨垂直轴位置状态
        [ObservableProperty]
        private bool _clampVerticalUpperLimit = false;

        [ObservableProperty]
        private bool _clampVerticalLowerLimit = false;

        [ObservableProperty]
        private bool _clampVerticalOriginLimit = false;

        [ObservableProperty]
        private int _clampVerticalUpperLimitPosition = 1;

        [ObservableProperty]
        private int _clampVerticalLowerLimitPosition = 1;

        [ObservableProperty]
        private int _clampVerticalOriginPosition = 1;

        [ObservableProperty]
        private int _clampVerticalCurrentPosition = 1;

        [ObservableProperty]
        private int _clampVerticalCurrentSpeed = 1;

        // 炉内状态
        [ObservableProperty]
        private bool _furnaceStatus = false;

        public MotionPlcData() { }
    }

    public partial class MotionVM : ObservableObject
    {
        private DispatcherTimer DatabaseSaveTimer;
        private ModbusTcpNet modbusClient;
        [ObservableProperty]
        private MotionPlcData _motionPlcData;

        // 添加源位置和目标位置的属性
        [ObservableProperty]
        private string _selectedSourcePosition;

        [ObservableProperty]
        private string _selectedTargetPosition;

        // 添加移动命令
        [RelayCommand]
        private async Task ExecuteMove()
        {
            // 检查是否有选择位置
            if (string.IsNullOrEmpty(SelectedSourcePosition) || string.IsNullOrEmpty(SelectedTargetPosition))
            {
                MessageBox.Show("请选择源位置和目标位置");
                return;
            }

            // 检查机械手是否在运动
            if (IsRobotMoving())
            {
                MessageBox.Show("机械手正在运动中，请等待运动完成");
                return;
            }

            try
            {
                // 根据选择的位置生成命令代码
                byte commandCode = GetCommandCode(SelectedSourcePosition, SelectedTargetPosition);
                
                // 写入PLC
                await modbusClient.WriteAsync("442", true);  // 启动信号
                await modbusClient.WriteAsync("443", commandCode);  // 写入命令代码
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行移动命令失败: {ex.Message}");
            }
        }

        // 判断机械手是否在运动
        private bool IsRobotMoving()
        {
            // 只检查机械手三个轴的速度
            return _motionPlcData.RobotHorizontal1CurrentSpeed != 0 ||
                   _motionPlcData.RobotHorizontal2CurrentSpeed != 0 ||
                   _motionPlcData.RobotVerticalCurrentSpeed != 0;
        }

        // 添加夹爪运动检查方法
        private bool IsClampMoving()
        {
            // 检查夹爪两个轴的速度
            return _motionPlcData.ClampHorizontalCurrentSpeed != 0 ||
                   _motionPlcData.ClampVerticalCurrentSpeed != 0;
        }

        private byte GetCommandCode(string source, string target)
        {
            // 根据源位置和目标位置的组合返回对应的命令代码
            // 这里需要根据您的实际PLC程序定义来设置具体的命令代码
            Dictionary<(string, string), byte> commandMap = new Dictionary<(string, string), byte>
            {
                { ("小车区", "存储区1"), 1 },
                { ("小车区", "存储区2"), 2 },
                { ("小车区", "桨区"), 3 },
                { ("存储区1", "小车区"), 4 },
                { ("存储区1", "存储区2"), 5 },
                { ("存储区1", "桨区"), 6 },
                { ("存储区2", "小车区"), 7 },
                { ("存储区2", "存储区1"), 8 },
                { ("存储区2", "桨区"), 9 },
                { ("桨区", "小车区"), 10 },
                { ("桨区", "存储区1"), 11 },
                { ("桨区", "存储区2"), 12 },
            };

            if (commandMap.TryGetValue((source, target), out byte code))
            {
                return code;
            }

            throw new ArgumentException("无效的位置组合");
        }

        // 添加控制命令
        [RelayCommand]
        private async Task MoveRobotHorizontal1ToForwardLimit()
        {
            try
            {
                // 如果机械手在运动，先停止当前动作
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("200", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotHorizontal1ToBackwardLimit()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("201", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotHorizontal2ToForwardLimit()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("202", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotHorizontal2ToBackwardLimit()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("203", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotVerticalToUpperLimit()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("204", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotVerticalToLowerLimit()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("205", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotVerticalToOrigin()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("206", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotHorizontal1ToOrigin()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("207", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveRobotHorizontal2ToOrigin()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("208", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveAllToOrigin()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("209", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveFurnaceUp()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("210", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task MoveFurnaceOut()
        {
            try
            {
                if (IsRobotMoving())
                {
                    await StopAllMotion();
                }
                await modbusClient.WriteAsync("211", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 添加停止所有运动的方法
        private async Task StopAllMotion()
        {
            try
            {
                // 写入停止信号到PLC
                await modbusClient.WriteAsync("459", true);
                // 等待一小段时间确保停止命令被执行
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止运动失败: {ex.Message}");
            }
        }
        //速度模式和运动模式的选择
        [ObservableProperty]
        private bool _isSpeedMode = true;

        [ObservableProperty]
        private string _inputLabel = "速度值";

        [RelayCommand]
        private void SwitchToSpeedMode()
        {
            IsSpeedMode = true;
            InputLabel = "速度值";
        }

        [RelayCommand]
        private void SwitchToPositionMode()
        {
            IsSpeedMode = false;
            InputLabel = "位置值";
        }

        // 轴选择状态
        [ObservableProperty]
        private bool _isHorizontal1Selected = false;

        [ObservableProperty]
        private bool _isHorizontal2Selected = false;

        [ObservableProperty]
        private bool _isVerticalSelected = false;

        // 输入的数值
        [ObservableProperty]
        private int _inputValue = 0;

        // 启动命令
        [RelayCommand]
        private async Task StartMotion()
        {
            try
            {
                // 检查是否有选择轴
                if (!IsHorizontal1Selected && !IsHorizontal2Selected && !IsVerticalSelected)
                {
                    MessageBox.Show("请至少选择一个轴");
                    return;
                }

                // 检查输入值是否合法
                if (InputValue <= 0)
                {
                    MessageBox.Show("请输入大于0的值");
                    return;
                }

                // 根据选择的轴和模式，发送对应的命令
                if (IsSpeedMode)
                {
                    // 速度模式下发命令
                    if (IsHorizontal1Selected)
                    {
                        // 写入水平一轴速度值和启动信号
                        await modbusClient.WriteAsync("445", InputValue);  // 速度值地址
                        await modbusClient.WriteAsync("448", true);        // 启动信号地址
                    }
                    if (IsHorizontal2Selected)
                    {
                        // 写入水平二轴速度值和启动信号
                        await modbusClient.WriteAsync("446", InputValue);
                        await modbusClient.WriteAsync("448", true);
                    }
                    if (IsVerticalSelected)
                    {
                        // 写入垂直轴速度值和启动信号
                        await modbusClient.WriteAsync("447", InputValue);
                        await modbusClient.WriteAsync("448", true);
                    }
                }
                else
                {
                    // 位置模式下发命令
                    if (IsHorizontal1Selected)
                    {
                        // 写入水平一轴位置值和启动信号
                        await modbusClient.WriteAsync("449", InputValue);  // 位置值地址
                        await modbusClient.WriteAsync("452", true);        // 启动信号地址
                    }
                    if (IsHorizontal2Selected)
                    {
                        // 写入水平二轴位置值和启动信号
                        await modbusClient.WriteAsync("450", InputValue);
                        await modbusClient.WriteAsync("452", true);
                    }
                    if (IsVerticalSelected)
                    {
                        // 写入垂直轴位置值和启动信号
                        await modbusClient.WriteAsync("451", InputValue);
                        await modbusClient.WriteAsync("452", true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}");
            }
        }

        #region 桨精确控制属性和命令

        // 桨轴选择状态
        [ObservableProperty]
        private bool _isClampHorizontalSelected = false;  // 桨水平轴选择状态

        [ObservableProperty]
        private bool _isClampVerticalSelected = false;    // 桨垂直轴选择状态

        // 桨运动模式
        [ObservableProperty]
        private bool _isClampSpeedMode = true;           // true: 速度模式, false: 位置模式

        // 桨输入值标签
        [ObservableProperty]
        private string _clampInputLabel = "速度值：";     // 根据模式显示"速度值："或"位置值："

        // 桨输入的数值
        [ObservableProperty]
        private int _clampInputValue = 0;                // 输入的速度值或位置值

        /// <summary>
        /// 切换到桨速度模式
        /// </summary>
        [RelayCommand]
        private void SwitchToClampSpeedMode()
        {
            IsClampSpeedMode = true;
            ClampInputLabel = "速度值：";
        }

        /// <summary>
        /// 切换到桨位置模式
        /// </summary>
        [RelayCommand]
        private void SwitchToClampPositionMode()
        {
            IsClampSpeedMode = false;
            ClampInputLabel = "位置值：";
        }

        /// <summary>
        /// 启动桨运动命令
        /// </summary>
        [RelayCommand]
        private async Task StartClampMotion()
        {
            try
            {
                // 1. 检查是否选择了轴
                if (!IsClampHorizontalSelected && !IsClampVerticalSelected)
                {
                    MessageBox.Show("请至少选择一个轴");
                    return;
                }

                // 2. 检查输入值是否合法
                if (ClampInputValue <= 0)
                {
                    MessageBox.Show("请输入大于0的值");
                    return;
                }

                // 3. 根据选择的轴和模式发送命令
                if (IsClampSpeedMode)
                {
                    // 速度模式下发命令
                    if (IsClampHorizontalSelected)
                    {
                        // 写入桨水平轴速度值和启动信号
                        await modbusClient.WriteAsync("455", ClampInputValue);  // 速度值地址
                        await modbusClient.WriteAsync("453", true);             // 启动信号地址
                    }
                    if (IsClampVerticalSelected)
                    {
                        // 写入桨垂直轴速度值和启动信号
                        await modbusClient.WriteAsync("455", ClampInputValue);  // 速度值地址
                        await modbusClient.WriteAsync("454", true);             // 启动信号地址
                    }
                }
                else
                {
                    // 位置模式下发命令
                    if (IsClampHorizontalSelected)
                    {
                        // 写入桨水平轴位置值和启动信号
                        await modbusClient.WriteAsync("456", ClampInputValue);  // 位置值地址
                        await modbusClient.WriteAsync("458", true);             // 启动信号地址
                    }
                    if (IsClampVerticalSelected)
                    {
                        // 写入桨垂直轴位置值和启动信号
                        await modbusClient.WriteAsync("457", ClampInputValue);  // 位置值地址
                        await modbusClient.WriteAsync("458", true);             // 启动信号地址
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}");
            }
        }

        #endregion

        #region 桨定点移动命令

        // 桨水平轴到前限位
        [RelayCommand]
        private async Task ClampHorizontalToFrontLimit()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100); // 等待停止完成
                }
                // 发送命令到PLC - 桨水平轴到前限位
                await modbusClient.WriteAsync("424", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 桨水平轴到后限位
        [RelayCommand]
        private async Task ClampHorizontalToBackLimit()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100);
                }
                // 发送命令到PLC - 桨水平轴到后限位
                await modbusClient.WriteAsync("425", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 桨垂直轴到上限位
        [RelayCommand]
        private async Task ClampVerticalToUpperLimit()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100);
                }
                // 发送命令到PLC - 桨垂直轴到上限位
                await modbusClient.WriteAsync("426", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 桨垂直轴到下限位
        [RelayCommand]
        private async Task ClampVerticalToLowerLimit()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100);
                }
                // 发送命令到PLC - 桨垂直轴到下限位
                await modbusClient.WriteAsync("427", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 桨垂直轴到原点
        [RelayCommand]
        private async Task ClampVerticalToOrigin()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100);
                }
                // 发送命令到PLC - 桨垂直轴到原点
                await modbusClient.WriteAsync("428", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 桨水平轴到原点
        [RelayCommand]
        private async Task ClampHorizontalToOrigin()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100);
                }
                // 发送命令到PLC - 桨水平轴到原点
                await modbusClient.WriteAsync("429", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        // 桨整体回原点
        [RelayCommand]
        private async Task ClampAllToOrigin()
        {
            try
            {
                if (IsClampMoving())
                {
                    await StopAllMotion();
                    await Task.Delay(100);
                }
                // 发送命令到PLC - 桨整体回原点
                await modbusClient.WriteAsync("430", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        #endregion

        public MotionVM()
        {
            // 订阅连接成功事件
            GlobalVM.SingleObject.OnConnected += OnPlcConnectedAsync;

            // 初始化定时器
            DatabaseSaveTimer = new DispatcherTimer();
            DatabaseSaveTimer.Interval = TimeSpan.FromMilliseconds(100);
            DatabaseSaveTimer.Tick += DatabaseSaveTimer_Tick;

            // 初始化 ModbusTcp 客户端和数据对象
            modbusClient = GlobalVM.plcCommunicationService._modbusTcp;
            _motionPlcData = new MotionPlcData();
        }

        private async Task<Task> OnPlcConnectedAsync()
        {
            // PLC连接成功后启动定时器
            DatabaseSaveTimer.Start();
            return Task.CompletedTask;
        }

        private void DatabaseSaveTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 读取线圈状态
                _motionPlcData.Door1Lock = modbusClient.ReadCoil("1").Content;
                _motionPlcData.Door2Lock = modbusClient.ReadCoil("2").Content;
                _motionPlcData.FurnaceVerticalCylinder = modbusClient.ReadCoil("3").Content;
                _motionPlcData.FurnaceHorizontalCylinder = modbusClient.ReadCoil("4").Content;
                _motionPlcData.Storage1HasMaterial = modbusClient.ReadCoil("5").Content;
                _motionPlcData.Storage2HasMaterial = modbusClient.ReadCoil("6").Content;
                _motionPlcData.ClampHasMaterial = modbusClient.ReadCoil("7").Content;

                // 机械手水平一轴
                _motionPlcData.RobotHorizontal1ForwardLimit = modbusClient.ReadCoil("8").Content;
                _motionPlcData.RobotHorizontal1BackwardLimit = modbusClient.ReadCoil("9").Content;
                _motionPlcData.RobotHorizontal1OriginLimit = modbusClient.ReadCoil("10").Content;
                _motionPlcData.RobotHorizontal1UpperLimit = modbusClient.ReadInt32("11").Content;
                _motionPlcData.RobotHorizontal1LowerLimit = modbusClient.ReadInt32("12").Content;
                _motionPlcData.RobotHorizontal1OriginPosition = modbusClient.ReadInt32("13").Content;
                _motionPlcData.RobotHorizontal1CurrentPosition = modbusClient.ReadInt32("14").Content;
                _motionPlcData.RobotHorizontal1CurrentSpeed = modbusClient.ReadInt32("15").Content;

                // 机械手水平二轴
                _motionPlcData.RobotHorizontal2ForwardLimit = modbusClient.ReadCoil("16").Content;
                _motionPlcData.RobotHorizontal2BackwardLimit = modbusClient.ReadCoil("17").Content;
                _motionPlcData.RobotHorizontal2OriginLimit = modbusClient.ReadCoil("18").Content;
                _motionPlcData.RobotHorizontal2UpperLimit = modbusClient.ReadInt32("19").Content;
                _motionPlcData.RobotHorizontal2LowerLimit = modbusClient.ReadInt32("20").Content;
                _motionPlcData.RobotHorizontal2OriginPosition = modbusClient.ReadInt32("21").Content;
                _motionPlcData.RobotHorizontal2CurrentPosition = modbusClient.ReadInt32("22").Content;
                _motionPlcData.RobotHorizontal2CurrentSpeed = modbusClient.ReadInt32("23").Content;

                // 机械手垂直轴
                _motionPlcData.RobotVerticalUpperLimit = modbusClient.ReadCoil("24").Content;
                _motionPlcData.RobotVerticalLowerLimit = modbusClient.ReadCoil("25").Content;
                _motionPlcData.RobotVerticalOriginLimit = modbusClient.ReadCoil("26").Content;
                _motionPlcData.RobotVerticalUpperLimitPosition = modbusClient.ReadInt32("27").Content;
                _motionPlcData.RobotVerticalLowerLimitPosition = modbusClient.ReadInt32("28").Content;
                _motionPlcData.RobotVerticalOriginPosition = modbusClient.ReadInt32("29").Content;
                _motionPlcData.RobotVerticalCurrentPosition = modbusClient.ReadInt32("30").Content;
                _motionPlcData.RobotVerticalCurrentSpeed = modbusClient.ReadInt32("31").Content;

                // 桨水平轴
                _motionPlcData.ClampHorizontalForwardLimit = modbusClient.ReadCoil("32").Content;
                _motionPlcData.ClampHorizontalBackwardLimit = modbusClient.ReadCoil("33").Content;
                _motionPlcData.ClampHorizontalOriginLimit = modbusClient.ReadCoil("34").Content;
                _motionPlcData.ClampHorizontalUpperLimit = modbusClient.ReadInt32("35").Content;
                _motionPlcData.ClampHorizontalLowerLimit = modbusClient.ReadInt32("36").Content;
                _motionPlcData.ClampHorizontalOriginPosition = modbusClient.ReadInt32("37").Content;
                _motionPlcData.ClampHorizontalCurrentPosition = modbusClient.ReadInt32("38").Content;
                _motionPlcData.ClampHorizontalCurrentSpeed = modbusClient.ReadInt32("39").Content;

                // 桨垂直轴
                _motionPlcData.ClampVerticalUpperLimit = modbusClient.ReadCoil("40").Content;
                _motionPlcData.ClampVerticalLowerLimit = modbusClient.ReadCoil("41").Content;
                _motionPlcData.ClampVerticalOriginLimit = modbusClient.ReadCoil("42").Content;
                _motionPlcData.ClampVerticalUpperLimitPosition = modbusClient.ReadInt32("43").Content;
                _motionPlcData.ClampVerticalLowerLimitPosition = modbusClient.ReadInt32("44").Content;
                _motionPlcData.ClampVerticalOriginPosition = modbusClient.ReadInt32("45").Content;
                _motionPlcData.ClampVerticalCurrentPosition = modbusClient.ReadInt32("46").Content;
                _motionPlcData.ClampVerticalCurrentSpeed = modbusClient.ReadInt32("47").Content;

                // 炉内状态
                _motionPlcData.FurnaceStatus = modbusClient.ReadCoil("48").Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Modbus通讯异常: {ex.Message}");
            }
        }

        public void Cleanup()
        {
            GlobalVM.SingleObject.OnConnected -= OnPlcConnectedAsync;
            DatabaseSaveTimer.Stop();
        }
    }
}
