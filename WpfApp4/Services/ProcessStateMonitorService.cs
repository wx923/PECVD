using System;
using System.Linq;
using System.Windows;
using WpfApp4.Models;
using WpfApp4.ViewModel;

namespace WpfApp4.Services
{
    /// <summary>
    /// 监控PLC状态变化并处理相关业务逻辑的服务
    /// </summary>
    public class ProcessStateMonitorService
    {
        private static readonly Lazy<ProcessStateMonitorService> _instance = 
            new Lazy<ProcessStateMonitorService>(() => new ProcessStateMonitorService());
        public static ProcessStateMonitorService Instance => _instance.Value;

        private ProcessStateMonitorService()
        {
            // 订阅小车状态变化事件
            MotionPlcDataService.Instance.CarriageArrivedWithMaterial += OnCarriageArrivedWithMaterial;
            MotionPlcDataService.Instance.MaterialRemovedFromCarriage += OnMaterialRemovedFromCarriage;
            MotionPlcDataService.Instance.MaterialReturnedToCarriage += OnMaterialReturnedToCarriage;
            MotionPlcDataService.Instance.CarriageLeftWithoutMaterial += OnCarriageLeftWithoutMaterial;

            // 这里可以添加其他PLC状态变化的事件订阅
        }

        #region 小车状态变化处理
        private async void OnCarriageArrivedWithMaterial(object sender, EventArgs e)
        {
            try
            {
                // 创建新的舟对象
                var boat = new Boat
                {
                    // 设置舟的属性
                    BoatNumber = GenerateBoatNumber(),
                    CreateTime = DateTime.Now,
                    Status = "待处理"
                };
                await GlobalVM.AddBoatAsync(boat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建舟对象失败: {ex.Message}");
            }
        }

        private void OnMaterialRemovedFromCarriage(object sender, EventArgs e)
        {
            // 处理料被移走的逻辑
            // 例如：更新舟的状态
            UpdateBoatLocation("存储区");
        }

        private void OnMaterialReturnedToCarriage(object sender, EventArgs e)
        {
            // 处理料回到小车的逻辑
            // 例如：更新舟的状态为准备出料
            UpdateBoatLocation("小车区");
        }

        private void OnCarriageLeftWithoutMaterial(object sender, EventArgs e)
        {
            // 处理小车空车离开的逻辑
            // 例如：完成舟的生命周期
            CompleteBoatProcess();
        }

        private string GenerateBoatNumber()
        {
            // 生成舟编号的逻辑
            return $"BOAT_{DateTime.Now:yyyyMMddHHmmss}";
        }

        private void UpdateBoatLocation(string location)
        {
            // 更新舟位置的逻辑
            var currentBoat = GlobalVM.GlobalBoats.FirstOrDefault(b => b.Status == "待处理");
            if (currentBoat != null)
            {
                currentBoat.Location = location;
            }
        }

        private void CompleteBoatProcess()
        {
            // 完成舟处理的逻辑
            var currentBoat = GlobalVM.GlobalBoats.FirstOrDefault(b => b.Status == "待处理");
            if (currentBoat != null)
            {
                currentBoat.Status = "已完成";
                currentBoat.CompleteTime = DateTime.Now;
            }
        }
        #endregion

        #region 其他PLC状态变化处理方法
        // 这里可以添加其他PLC状态变化的处理方法
        #endregion

        public void Cleanup()
        {
            var service = MotionPlcDataService.Instance;
            service.CarriageArrivedWithMaterial -= OnCarriageArrivedWithMaterial;
            service.MaterialRemovedFromCarriage -= OnMaterialRemovedFromCarriage;
            service.MaterialReturnedToCarriage -= OnMaterialReturnedToCarriage;
            service.CarriageLeftWithoutMaterial -= OnCarriageLeftWithoutMaterial;

            // 取消订阅其他事件...
        }
    }
} 