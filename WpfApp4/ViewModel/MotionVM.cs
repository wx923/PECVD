using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HslCommunication.ModBus;
using WpfApp.Services;
using WpfApp4.Models;
using WpfApp4.Services;

namespace WpfApp4.ViewModel
{
    public partial class MotionVM : ObservableObject
    {
        private ModbusTcpNet _modbusClient => PlcCommunicationService.Instance.ModbusTcpClients[PlcCommunicationService.PlcType.Motion];
        private MotionPlcDataService _plcDataService => MotionPlcDataService.Instance;
        
        // 获取PLC数据的属性
        public MotionPlcData MotionPlcData => _plcDataService.MotionPlcData;

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
                await _modbusClient.WriteAsync("442", true);  // 启动信号
                await _modbusClient.WriteAsync("443", commandCode);  // 写入命令代码
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行移动命令失败: {ex.Message}");
            }
        }

        // 判断机械手是否在运动
        private bool IsRobotMoving()
        {
            return MotionPlcData.RobotHorizontal1CurrentSpeed != 0 ||
                   MotionPlcData.RobotHorizontal2CurrentSpeed != 0 ||
                   MotionPlcData.RobotVerticalCurrentSpeed != 0;
        }

        // 添加夹爪运动检查方法
        private bool IsClampMoving()
        {
            return MotionPlcData.ClampHorizontalCurrentSpeed != 0 ||
                   MotionPlcData.ClampVerticalCurrentSpeed != 0;
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
                await _modbusClient.WriteAsync("200", true);
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
                await _modbusClient.WriteAsync("201", true);
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
                await _modbusClient.WriteAsync("202", true);
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
                await _modbusClient.WriteAsync("203", true);
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
                await _modbusClient.WriteAsync("204", true);
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
                await _modbusClient.WriteAsync("205", true);
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
                await _modbusClient.WriteAsync("206", true);
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
                await _modbusClient.WriteAsync("207", true);
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
                await _modbusClient.WriteAsync("208", true);
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
                await _modbusClient.WriteAsync("209", true);
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
                await _modbusClient.WriteAsync("210", true);
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
                await _modbusClient.WriteAsync("211", true);
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
                await _modbusClient.WriteAsync("459", true);
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
                        await _modbusClient.WriteAsync("445", InputValue);  // 速度值地址
                        await _modbusClient.WriteAsync("448", true);        // 启动信号地址
                    }
                    if (IsHorizontal2Selected)
                    {
                        // 写入水平二轴速度值和启动信号
                        await _modbusClient.WriteAsync("446", InputValue);
                        await _modbusClient.WriteAsync("448", true);
                    }
                    if (IsVerticalSelected)
                    {
                        // 写入垂直轴速度值和启动信号
                        await _modbusClient.WriteAsync("447", InputValue);
                        await _modbusClient.WriteAsync("448", true);
                    }
                }
                else
                {
                    // 位置模式下发命令
                    if (IsHorizontal1Selected)
                    {
                        // 写入水平一轴位置值和启动信号
                        await _modbusClient.WriteAsync("449", InputValue);  // 位置值地址
                        await _modbusClient.WriteAsync("452", true);        // 启动信号地址
                    }
                    if (IsHorizontal2Selected)
                    {
                        // 写入水平二轴位置值和启动信号
                        await _modbusClient.WriteAsync("450", InputValue);
                        await _modbusClient.WriteAsync("452", true);
                    }
                    if (IsVerticalSelected)
                    {
                        // 写入垂直轴位置值和启动信号
                        await _modbusClient.WriteAsync("451", InputValue);
                        await _modbusClient.WriteAsync("452", true);
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
                        await _modbusClient.WriteAsync("455", ClampInputValue);  // 速度值地址
                        await _modbusClient.WriteAsync("453", true);             // 启动信号地址
                    }
                    if (IsClampVerticalSelected)
                    {
                        // 写入桨垂直轴速度值和启动信号
                        await _modbusClient.WriteAsync("455", ClampInputValue);  // 速度值地址
                        await _modbusClient.WriteAsync("454", true);             // 启动信号地址
                    }
                }
                else
                {
                    // 位置模式下发命令
                    if (IsClampHorizontalSelected)
                    {
                        // 写入桨水平轴位置值和启动信号
                        await _modbusClient.WriteAsync("456", ClampInputValue);  // 位置值地址
                        await _modbusClient.WriteAsync("458", true);             // 启动信号地址
                    }
                    if (IsClampVerticalSelected)
                    {
                        // 写入桨垂直轴位置值和启动信号
                        await _modbusClient.WriteAsync("457", ClampInputValue);  // 位置值地址
                        await _modbusClient.WriteAsync("458", true);             // 启动信号地址
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
                await _modbusClient.WriteAsync("424", true);
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
                await _modbusClient.WriteAsync("425", true);
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
                await _modbusClient.WriteAsync("426", true);
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
                await _modbusClient.WriteAsync("427", true);
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
                await _modbusClient.WriteAsync("428", true);
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
                await _modbusClient.WriteAsync("429", true);
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
                await _modbusClient.WriteAsync("430", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"移动失败: {ex.Message}");
            }
        }

        #endregion

        public MotionVM()
        {
            // 删除这些行（如果存在）：
            // modbusClient = ...
            // _plcDataService = new MotionPlcDataService();
            
            // 其他初始化代码保持不变...
        }
    }
}
