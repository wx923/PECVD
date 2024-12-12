using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WpfApp4.Models;
using WpfApp4.Services;
using System.Windows;
using System.Linq;

namespace WpfApp4.ViewModel
{
    internal class GlobalVM
    {
        #region 单例和服务
        static public GlobalVM SingleObject { get; private set; }
        static public MongoDbService mongoDbService;
        #endregion

        #region 事件
        #endregion

        #region 全局数据集合
        public static ObservableCollection<Boat> GlobalBoats { get; set; } = new ObservableCollection<Boat>();
        public static ObservableCollection<BoatMonitor> GlobalMonitors { get; set; } = new ObservableCollection<BoatMonitor>();
        public static ObservableCollection<Process> GlobalProcesses { get; set; } = new ObservableCollection<Process>();
        public static ObservableCollection<ProcessReservation> GlobalReservations { get; set; } = new ObservableCollection<ProcessReservation>();
        #endregion

        #region 构造函数
        static GlobalVM()
        {
            InitializeCollections();
            mongoDbService = new MongoDbService();
            SingleObject = new GlobalVM();


        }

        private GlobalVM()
        {
            _ = InitializeAsync();
        }

        private static void InitializeCollections()
        {
            GlobalBoats = new ObservableCollection<Boat>();
            GlobalMonitors = new ObservableCollection<BoatMonitor>();
            GlobalProcesses = new ObservableCollection<Process>();
            GlobalReservations = new ObservableCollection<ProcessReservation>();
        }
        #endregion

        #region 初始化和数据加载
        private async Task InitializeAsync()
        {
            try
            {
                // 加载数据
                await LoadAllDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task LoadAllDataAsync()
        {
            try
            {
                // 清空现有数据
                ClearAllData();

                // 从数据库加载数据
                var boats = await mongoDbService.GetAllBoatsAsync();
                var monitors = await mongoDbService.GetAllBoatMonitorsAsync();
                var processes = await mongoDbService.GetAllProcessesAsync();
                var reservations = await mongoDbService.GetAllProcessReservationsAsync();

                // 更新集合
                foreach (var boat in boats)
                {
                    boat.PropertyChanged += async (s, e) =>
                    {
                        if (s is Boat b)
                        {
                            await mongoDbService.UpdateBoatAsync(b);
                        }
                    };
                    GlobalBoats.Add(boat);
                }

                foreach (var monitor in monitors)
                {
                    monitor.PropertyChanged += async (s, e) =>
                    {
                        if (s is BoatMonitor m)
                        {
                            await mongoDbService.UpdateBoatMonitorAsync(m);
                        }
                    };
                    GlobalMonitors.Add(monitor);
                }

                foreach (var process in processes)
                {
                    process.PropertyChanged += async (s, e) =>
                    {
                        if (s is Process p)
                        {
                            await mongoDbService.UpdateProcessAsync(p);
                        }
                    };
                    GlobalProcesses.Add(process);
                }

                foreach (var reservation in reservations)
                {
                    reservation.PropertyChanged += async (s, e) =>
                    {
                        if (s is ProcessReservation r)
                        {
                            await mongoDbService.UpdateProcessReservationAsync(r);
                        }
                    };
                    GlobalReservations.Add(reservation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 数据操作方法
        public static async Task AddBoatAsync(Boat boat)
        {
            try
            {
                await mongoDbService.AddBoatAsync(boat);
                boat.PropertyChanged += async (s, e) =>
                {
                    if (s is Boat b)
                    {
                        await mongoDbService.UpdateBoatAsync(b);
                    }
                };
                GlobalBoats.Add(boat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加舟对象失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task AddBoatMonitorAsync(BoatMonitor monitor)
        {
            try
            {
                await mongoDbService.AddBoatMonitorAsync(monitor);
                monitor.PropertyChanged += async (s, e) =>
                {
                    if (s is BoatMonitor m)
                    {
                        await mongoDbService.UpdateBoatMonitorAsync(m);
                    }
                };
                if (!GlobalMonitors.Any(m => m.BoatNumber == monitor.BoatNumber))
                {
                    GlobalMonitors.Add(monitor);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加监控对象失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static async Task DeleteBoatMonitorAsync(string boatNumber)
        {
            try
            {
                var monitor = GlobalMonitors.FirstOrDefault(m => m.BoatNumber == boatNumber);
                if (monitor != null)
                {
                    await mongoDbService.DeleteBoatMonitorAsync(boatNumber);
                    GlobalMonitors.Remove(monitor);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除监控对象失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region 清除全局数据
        public static void ClearAllData()
        {
            GlobalBoats?.Clear();
            GlobalMonitors?.Clear();
            GlobalProcesses?.Clear();
            GlobalReservations?.Clear();
        }
        #endregion
    }
}
