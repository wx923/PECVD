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
using System.Collections.Generic;
using WpfApp.Services;

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

        // 添加工艺文件相关属性
        [ObservableProperty]
        private ObservableCollection<ProcessFileInfo> _processFiles = new ObservableCollection<ProcessFileInfo>();

        #endregion

        #region 构造函数
        public BoatManagementVM()
        {
            InitializeCollections();
            SaveOriginalCollectionNames();  // 保存初始状态
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
                ProcessFiles = new ObservableCollection<ProcessFileInfo>(await MongoDbService.Instance.GetAllProcessFilesAsync());
                UpdateOperationStatus("数据加载成功", true);
            }
        }
        #endregion

        #region 工艺文件操作
        [RelayCommand]
        private async Task SaveProcessFiles()
        {
            try
            {
                var selectedFiles = ProcessFiles.Where(f => f.IsSelected).ToList();
                if (!selectedFiles.Any())
                {
                    UpdateOperationStatus("请先选择要修改的工艺文件", false);
                    return;
                }

                foreach (var processFile in selectedFiles)
                {
                    // 检查新的集合名是否已存在
                    var newCollectionName = processFile.FileName.Replace(" ", "_")
                                                              .Replace("-", "_")
                                                              .Replace(".", "_")
                                                              .Replace("/", "_")
                                                              .Replace("\\", "_");

                    // 获取原始工艺文件信息
                    var processData = await MongoDbService.Instance.GetProcessDataByFileIdAsync(processFile.Id);

                    // 如果文件名发生改变，需要重命名集合
                    if (processFile.CollectionName != newCollectionName)
                    {
                        // 检查新集合名是否已存在
                        if (await MongoDbService.Instance.CollectionExistsAsync(newCollectionName))
                        {
                            UpdateOperationStatus($"已存在名为 {newCollectionName} 的集合", false);
                            continue;
                        }

                        // 重命名集合
                        await MongoDbService.Instance.RenameCollectionAsync(processFile.CollectionName, newCollectionName);
                    }

                    // 更新工艺文件信息
                    await MongoDbService.Instance.SaveProcessFileAsync(
                        processFile.FileName,
                        processFile.Description,
                        processData
                    );
                    
                    // 更新本地列表
                    var index = ProcessFiles.IndexOf(processFile);
                    if (index != -1)
                    {
                        processFile.CollectionName = newCollectionName;
                        processFile.createTime = DateTime.Now;
                        ProcessFiles[index] = processFile;
                    }
                }

                UpdateOperationStatus($"已成功保存 {selectedFiles.Count} 个工艺文件", true);
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"保存工艺文件失败: {ex.Message}", false);
            }
        }

        [RelayCommand]
        private async Task DeleteSelectedProcessFile()
        {
            try
            {
                var selectedFiles = ProcessFiles.Where(f => f.IsSelected).ToList();
                if (!selectedFiles.Any())
                {
                    UpdateOperationStatus("请先选择要删除的工艺文件", false);
                    return;
                }

                var result = MessageBox.Show($"确定要删除选中的 {selectedFiles.Count} 个工艺文件吗？", 
                    "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result != MessageBoxResult.Yes) return;

                foreach (var processFile in selectedFiles)
                {
                    // 删除数据库中的工艺文件
                    await MongoDbService.Instance.DeleteProcessFileAsync(processFile.Id);
                    
                    // 从本地列表中移除
                    ProcessFiles.Remove(processFile);
                }
                
                UpdateOperationStatus($"已成功删除 {selectedFiles.Count} 个工艺文件", true);
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"删除工艺文件失败: {ex.Message}", false);
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

                // 提交到数据库
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
                UpdateOperationStatus("舟象修改已保存", true);
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"保存舟对象修改失败: {ex.Message}", false);
            }
        }

        #region 炉管工艺操作
        public ObservableCollection<FurnaceData> Furnaces => FurnaceService.Instance.Furnaces;

        // 用于存储原始的CollectionName，用于比较是否有修改
        private string[] _originalCollectionNames = new string[6];

        // 在初始化或加载数据时保存原始的CollectionName
        private void SaveOriginalCollectionNames()
        {
            for (int i = 0; i < 6; i++)
            {
                _originalCollectionNames[i] = Furnaces[i].ProcessCollectionName;
            }
        }

        [RelayCommand]
        private async Task UpdateFurnaceProcess()
        {
            try
            {
                var modifiedFurnaces = new List<(int Index, FurnaceData Furnace)>();
                
                // 检查每个炉管是否有修改
                for (int i = 0; i < 6; i++)
                {
                    if (_originalCollectionNames[i] != Furnaces[i].ProcessCollectionName)
                    {
                        modifiedFurnaces.Add((i, Furnaces[i]));
                    }
                }

                if (!modifiedFurnaces.Any())
                {
                    UpdateOperationStatus("没有炉管工艺被修改", false);
                    return;
                }

                foreach (var (index, furnace) in modifiedFurnaces)
                {
                    try
                    {
                        // 获取对应炉管的ModbusTcp客户端
                        var modbusClient = PlcCommunicationService.Instance.ModbusTcpClients[(PlcCommunicationService.PlcType)index];
                        
                        // 检查炉管是否在工艺中
                        var isInProcess = await modbusClient.ReadBoolAsync("1000");  // 假设1000地址存储工艺运行状态
                        if (isInProcess.Content)
                        {
                            MessageBox.Show($"炉管{index + 1}正在运行工艺，无法更新工艺文件！");
                            continue;
                        }

                        // 下发工艺文件到PLC
                        await FurnaceService.Instance.SendProcessDataToPLC(furnace.ProcessCollectionName, index + 1);

                        // 更新原始值
                        _originalCollectionNames[index] = furnace.ProcessCollectionName;
                        
                        UpdateOperationStatus($"炉管{index + 1}工艺更新成功", false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"炉管{index + 1}工艺更新失败: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateOperationStatus($"更新炉管工艺失败: {ex.Message}", false);
            }
        }
        #endregion
    }
} 