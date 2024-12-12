using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp4.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using WpfApp4.Services;

namespace WpfApp4.ViewModel
{
    public partial class BoatManagementVM : ObservableObject
    {
        #region 属性
        [ObservableProperty]
        private ObservableCollection<Boat> _boats;

        [ObservableProperty]
        private Boat _selectedBoat;

        [ObservableProperty]
        private ObservableCollection<BoatMonitor> _boatMonitors;

        [ObservableProperty]
        private BoatMonitor _selectedMonitor;

        [ObservableProperty]
        private bool _isDatabaseConnected;

        [ObservableProperty]
        private string _databaseStatus;

        [ObservableProperty]
        private string _lastOperationStatus;

        #endregion

        #region 构造函数
        public BoatManagementVM()
        {
            InitializeCollections();
            _ = InitializeAsync();
        }

        private void InitializeCollections()
        {
            Boats = new ObservableCollection<Boat>();
            BoatMonitors = new ObservableCollection<BoatMonitor>();
        }

        private async Task InitializeAsync()
        {
            await CheckDatabaseConnection();
            if (IsDatabaseConnected)
            {
                BoatMonitors = MongoDbService.Instance.GlobalMonitors;
                UpdateOperationStatus("数据加载成功", true);
            }
        }
        #endregion

        #region 舟监控对象操作
        [RelayCommand]
        private void AddEmptyRow()
        {
            var newMonitor = new BoatMonitor
            {
                ProcessStartTime = DateTime.Now,
                ProcessEndTime = DateTime.Now.AddHours(1),
                ProcessCount = 0,
                IsSubmitted = false  // 新行未提交
            };
            BoatMonitors.Add(newMonitor);
            UpdateOperationStatus("已添加新行，请填写舟号和当前工艺后点击提交", true);
        }

        [RelayCommand]
        private async Task SubmitNewRows()
        {
            try
            {
                // 获取所有未提交的行
                var newRows = BoatMonitors.Where(m => !m.IsSubmitted).ToList();
                var modifiedBoats = Boats.Where(b => b.IsModified).ToList();

                if (!newRows.Any() && !modifiedBoats.Any())
                {
                    UpdateOperationStatus("没有需要提交的新数据或修改", false);
                    return;
                }

                // 构建确认消息
                var confirmMessage = new StringBuilder();
                if (newRows.Any())
                {
                    confirmMessage.AppendLine($"新增监控对象：{newRows.Count} 个");
                    foreach (var row in newRows)
                    {
                        confirmMessage.AppendLine($"- 舟号：{row.BoatNumber}");
                    }
                }
                if (modifiedBoats.Any())
                {
                    if (confirmMessage.Length > 0) confirmMessage.AppendLine();
                    confirmMessage.AppendLine($"修改舟对象：{modifiedBoats.Count} 个");
                    foreach (var boat in modifiedBoats)
                    {
                        confirmMessage.AppendLine($"- 监控对象：{boat.MonitorBoatNumber}，位置：{boat.CurrentPosition}");
                    }
                }
                confirmMessage.AppendLine("\n是否确认提交这些更改？");

                // 显示确认对话框
                var result = MessageBox.Show(confirmMessage.ToString(), "确认提交", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    UpdateOperationStatus("已取消提交", false);
                    return;
                }

                // 验证新行数据
                var invalidRows = newRows.Where(m => string.IsNullOrWhiteSpace(m.BoatNumber)).ToList();
                if (invalidRows.Any())
                {
                    UpdateOperationStatus("存在未填写完整的行，请检查舟号是否已填写", false);
                    return;
                }

                // 检查舟号是否重复
                var existingBoatNumbers = BoatMonitors
                    .Where(m => m.IsSubmitted)
                    .Select(m => m.BoatNumber)
                    .ToList();

                var duplicateBoatNumbers = newRows
                    .Where(m => existingBoatNumbers.Contains(m.BoatNumber))
                    .Select(m => m.BoatNumber)
                    .ToList();

                if (duplicateBoatNumbers.Any())
                {
                    UpdateOperationStatus($"舟号 {string.Join(", ", duplicateBoatNumbers)} 已存在", false);
                    return;
                }

                // 提交新行到数据库
                foreach (var monitor in newRows)
                {
                    await MongoDbService.Instance.AddBoatMonitorAsync(monitor);
                    monitor.IsSubmitted = true;  // 更新提交状态
                }

                // 保存舟对象的修改
                foreach (var boat in modifiedBoats)
                {
                    await MongoDbService.Instance.UpdateBoatAsync(boat);
                    boat.IsModified = false;  // 重置修改标记
                }

                // 重新加载数据以确保显示最新状态
                await MongoDbService.Instance.LoadAllDataAsync();

                // 更新状态消息
                var successMessage = new StringBuilder();
                if (newRows.Any())
                    successMessage.Append($"成功提交 {newRows.Count} 个新的监控对象");
                if (modifiedBoats.Any())
                {
                    if (successMessage.Length > 0) successMessage.Append("，");
                    successMessage.Append($"成功保存 {modifiedBoats.Count} 个舟对象的修改");
                }
                UpdateOperationStatus(successMessage.ToString(), true);
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"提交失败: {ex.Message}", false);
            }
        }

        [RelayCommand]
        private async Task DeleteSelectedMonitors()
        {
            try
            {
                var selectedMonitors = BoatMonitors.Where(m => m.IsSelected).ToList();
                if (!selectedMonitors.Any())
                {
                    UpdateOperationStatus("请先选择要删除的监控对象", false);
                    return;
                }

                var result = MessageBox.Show($"确定要删除选中的 {selectedMonitors.Count} 个监控对象吗？",
                    "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (var monitor in selectedMonitors)
                    {
                        if (monitor.IsSubmitted)
                        {
                            // 只删除已经保存到数据库的象
                            await MongoDbService.Instance.DeleteBoatMonitorAsync(monitor._id);
                        }
                        BoatMonitors.Remove(monitor);
                    }
                    UpdateOperationStatus($"成功删除 {selectedMonitors.Count} 个监控对象", true);
                }
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"删除监控对象失败: {ex.Message}", false);
            }
        }
        #endregion

        #region 状态栏更新
        private async Task CheckDatabaseConnection()
        {
            try
            {
                await MongoDbService.Instance.GetAllBoatMonitorsAsync();
                IsDatabaseConnected = true;
                DatabaseStatus = "已连接";
                UpdateOperationStatus("数据库连接成功", true);
            }
            catch (Exception ex)
            {
                IsDatabaseConnected = false;
                DatabaseStatus = "未连接";
                UpdateOperationStatus($"数据库连接失败: {ex.Message}", false);
            }
        }

        private void UpdateOperationStatus(string message, bool isSuccess)
        {
            LastOperationStatus = $"{DateTime.Now:HH:mm:ss} - {message}";
            DatabaseStatus = isSuccess ? "已连接" : "未连接";
            IsDatabaseConnected = isSuccess;
        }

        // 刷新数据
        [RelayCommand]
        private async Task RefreshData()
        {
            try
            {
                await MongoDbService.Instance.LoadAllDataAsync();
                UpdateOperationStatus("数据刷新成功", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刷新数据失败: {ex.Message}");
            }
        }
        #endregion

        [RelayCommand]
        private async Task AddNewBoat()
        {
            try
            {
                var newBoat = new Boat
                {
                    CurrentPosition = BoatPosition.CarArea,
                    MonitorBoatNumber = string.Empty
                };

                Boats.Add(newBoat);
                UpdateOperationStatus("已添加新的舟对象，请选择监控对象后点击确认修改", true);
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"添加舟对象失败: {ex.Message}", false);
            }
        }

        [RelayCommand]
        private async Task SaveBoatChanges()
        {
            try
            {
                // 验证数据
                var invalidBoats = Boats.Where(b => string.IsNullOrWhiteSpace(b.MonitorBoatNumber)).ToList();
                if (invalidBoats.Any())
                {
                    UpdateOperationStatus("存在未选择监控对象的舟对象", false);
                    return;
                }

                // 保存所有修改
                foreach (var boat in Boats)
                {
                    await MongoDbService.Instance.UpdateBoatAsync(boat);
                }

                // 重新加载数据以确保显示最新状态
                await MongoDbService.Instance.LoadAllDataAsync();
                UpdateOperationStatus("舟对象修改已保存", true);
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"保存舟对象修改失败: {ex.Message}", false);
            }
        }
    }
} 