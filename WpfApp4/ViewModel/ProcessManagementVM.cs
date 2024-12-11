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

        private readonly MongoDbService _mongoDbService;

        private ProcessFileInfo _lastLoadedFile;  // 添加一个字段记录上次加载的文件

        public ProcessManagementVM()
        {
            _mongoDbService = GlobalVM.mongoDbService;
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
                var files = await _mongoDbService.GetAllProcessFilesAsync();
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
                    if (await _mongoDbService.CollectionExistsAsync(fileName))
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
                            ProcessName = GetCellValue(worksheet, row, 1),
                            N2O = GetCellValue(worksheet, row, 2),
                            H2 = GetCellValue(worksheet, row, 3),
                            Ph3 = GetCellValue(worksheet, row, 4),
                            Pressure = GetCellValue(worksheet, row, 5),
                            Power1 = GetCellValue(worksheet, row, 6),
                            Power2 = GetCellValue(worksheet, row, 7),
                            BoatInOut = GetCellValue(worksheet, row, 8),
                            MoveSpeed = GetCellValue(worksheet, row, 9),
                            UpDownSpeed = GetCellValue(worksheet, row, 10),
                            HeatTime = GetCellValue(worksheet, row, 11),
                            HeatTemp = GetCellValue(worksheet, row, 12),
                            PulseOn1 = GetCellValue(worksheet, row, 13),
                            PulseOff1 = GetCellValue(worksheet, row, 14),
                            PulseOn2 = GetCellValue(worksheet, row, 15),
                            PulseOff2 = GetCellValue(worksheet, row, 16),
                            CurrentVoltage = GetCellValue(worksheet, row, 17)
                        };

                        models.Add(model);
                    }

                    ExcelData = models;
                    
                    // 保存到新的集合中
                    await _mongoDbService.SaveProcessFileAsync(fileName, $"导入自Excel: {fileName}", models.ToList());
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
                await _mongoDbService.UpdateProcessExcelAsync(SelectedFile.Id, ExcelData.ToList());
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
                var data = await _mongoDbService.GetProcessDataByFileIdAsync(fileInfo.Id);
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