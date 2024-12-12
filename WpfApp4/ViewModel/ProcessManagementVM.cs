using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WpfApp4.Models;
using WpfApp4.Services;
using System.Linq;

namespace WpfApp4.ViewModel
{
    public partial class ProcessManagementVM : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProcessFileInfo> processFiles = new();

        [ObservableProperty]
        private ProcessFileInfo selectedFile;

        [ObservableProperty]
        private ObservableCollection<ProcessExcelModel> excelData = new();

        [ObservableProperty]
        private string operationStatus = "就绪";

        [ObservableProperty]
        private bool isLoading;

        private ProcessFileInfo _lastLoadedFile;  // 添加一个字段记录上次加载的文件

        public ProcessManagementVM()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _ = InitializeAsync();  // 异步初始化
        }

        private async Task InitializeAsync()
        {
            await LoadProcessFiles();
        }

        private async Task LoadProcessFiles()
        {
            try 
            {
                var files = await MongoDbService.Instance.GetAllProcessFilesAsync();
                ProcessFiles = new ObservableCollection<ProcessFileInfo>(files);
                
                // 如果有文件，默认选择第一个
                if (ProcessFiles.Any())
                {
                    SelectedFile = ProcessFiles.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载工艺文件列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ImportExcel()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "选择Excel文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    
                    // 检查MongoDB中是否存在同名集合
                    if (await MongoDbService.Instance.CollectionExistsAsync(fileName))
                    {
                        MessageBox.Show($"已存在名为 '{fileName}' 的工艺文件，请修改Excel文件名后重试。", 
                            "导入失败", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Warning);
                        return;
                    }

                    OperationStatus = "正在导入Excel...";
                    
                    using var package = new ExcelPackage(new FileInfo(openFileDialog.FileName));
                    var worksheet = package.Workbook.Worksheets[0]; // 获取第一个工作表

                    var rowCount = worksheet.Dimension.Rows;
                    var models = new ObservableCollection<ProcessExcelModel>();

                    // 从第二行开始读取（跳过表头）
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var model = new ProcessExcelModel
                        {
                            Time = GetCellIntValue(worksheet, row, 1),
                            // 温度参数 T1-T9
                            T1 = GetCellIntValue(worksheet, row, 2),
                            T2 = GetCellIntValue(worksheet, row, 3),
                            T3 = GetCellIntValue(worksheet, row, 4),
                            T4 = GetCellIntValue(worksheet, row, 5),
                            T5 = GetCellIntValue(worksheet, row, 6),
                            T6 = GetCellIntValue(worksheet, row, 7),
                            T7 = GetCellIntValue(worksheet, row, 8),
                            T8 = GetCellIntValue(worksheet, row, 9),
                            T9 = GetCellIntValue(worksheet, row, 10),
                            // 气体参数
                            N2 = GetCellIntValue(worksheet, row, 11),
                            SiH4 = GetCellIntValue(worksheet, row, 12),
                            N2O = GetCellIntValue(worksheet, row, 13),
                            H2 = GetCellIntValue(worksheet, row, 14),
                            Ph3 = GetCellIntValue(worksheet, row, 15),
                            // 压力参数
                            Pressure = GetCellIntValue(worksheet, row, 16),
                            // 功率参数
                            Power1 = GetCellIntValue(worksheet, row, 17),
                            Power2 = GetCellIntValue(worksheet, row, 18),
                            // 运动参数
                            BoatDirection = GetCellIntValue(worksheet, row, 19),
                            MoveSpeed = GetCellIntValue(worksheet, row, 20),
                            UpDownSpeed = GetCellIntValue(worksheet, row, 21),
                            // 加热参数
                            HeatTime = GetCellIntValue(worksheet, row, 22),
                            HeatTemp = GetCellIntValue(worksheet, row, 23),
                            // 脉冲参数
                            PulseOn1 = GetCellIntValue(worksheet, row, 24),
                            PulseOff1 = GetCellIntValue(worksheet, row, 25),
                            PulseOn2 = GetCellIntValue(worksheet, row, 26),
                            PulseOff2 = GetCellIntValue(worksheet, row, 27),
                            // 电流电压参数
                            CurrentReference = GetCellIntValue(worksheet, row, 28),
                            CurrentLimit = GetCellIntValue(worksheet, row, 29),
                            VoltageReference = GetCellIntValue(worksheet, row, 30),
                            VoltageLimit = GetCellIntValue(worksheet, row, 31)
                        };

                        models.Add(model);
                    }

                    ExcelData = models;
                    
                    // 保存到新的集合中
                    await MongoDbService.Instance.SaveProcessFileAsync(fileName, $"导入自Excel: {fileName}", models.ToList());
                    await LoadProcessFiles();
                    
                    // 自动选择新导入的文件
                    var newFile = ProcessFiles.FirstOrDefault(f => f.FileName == fileName);
                    if (newFile != null)
                    {
                        SelectedFile = newFile;
                    }
                    
                    OperationStatus = "Excel导入成功";
                }
                catch (Exception ex)
                {
                    OperationStatus = $"Excel导入失败: {ex.Message}";
                    MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col].Value;
            return cell?.ToString() ?? string.Empty;
        }

        private int GetCellIntValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = GetCellValue(worksheet, row, col);
            if (string.IsNullOrWhiteSpace(cellValue))
                return 0;
            
            if (int.TryParse(cellValue, out int result))
                return result;
                
            return 0;
        }

        [RelayCommand]
        private async Task SaveChanges()
        {
            try
            {
                if (SelectedFile == null)
                {
                    MessageBox.Show("请先选择一个工艺文件", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsLoading = true;
                OperationStatus = "正在保存更改...";
                await MongoDbService.Instance.UpdateProcessExcelAsync(SelectedFile.Id, ExcelData.ToList());
                OperationStatus = "保存成功";
            }
            catch (Exception ex)
            {
                OperationStatus = $"保存失败: {ex.Message}";
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProcessDataAsync(ProcessFileInfo fileInfo)
        {
            if (fileInfo == null) return;
            
            try
            {
                IsLoading = true;
                OperationStatus = "正在加载工艺数据...";
                var data = await MongoDbService.Instance.GetProcessDataByFileIdAsync(fileInfo.Id);
                ExcelData = new ObservableCollection<ProcessExcelModel>(data);
                OperationStatus = $"已加载工艺文件：{fileInfo.FileName}";
                _lastLoadedFile = fileInfo;  // 更新上次加载的文件记录
            }
            catch (Exception ex)
            {
                OperationStatus = $"加载失败: {ex.Message}";
                MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedFileChanged(ProcessFileInfo value)
        {
            if (value == null || IsLoading) return;
            if (_lastLoadedFile?.Id == value.Id) return;  // 如果是同一个文件，不重复加载
            
            _ = LoadProcessDataAsync(value);
        }
    }
} 