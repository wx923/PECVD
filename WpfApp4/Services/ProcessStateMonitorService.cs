using System;
using System.Linq;
using System.Windows;
using MongoDB.Bson;
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
                var boat = new Boat
                {
                    _id = ObjectId.GenerateNewId().ToString(),
                    CurrentPosition = BoatPosition.CarArea,
                    MonitorBoatNumber = GenerateBoatNumber(),
                };
                await MongoDbService.Instance.AddBoatAsync(boat);
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
            // 修改这里：使用 MongoDbService 的全局集合，并使用正确的属性
            var currentBoat = MongoDbService.Instance.GlobalBoats.FirstOrDefault(b => b.CurrentPosition == BoatPosition.CarArea);
            if (currentBoat != null)
            {
                // 根据 location 设置对应的 BoatPosition
            }
        }

        private void CompleteBoatProcess()
        {
            // 修改这里：使用 MongoDbService 的全局集合，并使用正确的属性
            var currentBoat = MongoDbService.Instance.GlobalBoats.FirstOrDefault(b => b.CurrentPosition == BoatPosition.CarArea);
            if (currentBoat != null)
            {
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