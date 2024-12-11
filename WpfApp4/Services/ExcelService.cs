using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using WpfApp4.Models;

namespace WpfApp4.Services
{
    public class ExcelService
    {
        public List<ProcessExcelModel> ImportExcel(string filePath)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0];

                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                // 验证列数是否正确
                if (colCount != 16)
                {
                    throw new Exception("Excel文件格式不正确，请确保包含16列数据");
                }

                // 验证表头是否正确
                for (int col = 1; col <= colCount; col++)
                {
                    var headerValue = worksheet.Cells[1, col].Value?.ToString();
                    if (string.IsNullOrEmpty(headerValue))
                    {
                        throw new Exception($"第{col}列的表头为空");
                    }
                }

                var dataList = new List<ProcessExcelModel>();

                // 从第二行开始读取数据（跳过表头）
                for (int row = 2; row <= rowCount; row++)
                {
                    var rowData = new ProcessExcelModel
                    {
                        N2O = GetCellValue(worksheet, row, 1),
                        H2 = GetCellValue(worksheet, row, 2),
                        Ph3 = GetCellValue(worksheet, row, 3),
                        Pressure = GetCellValue(worksheet, row, 4),
                        Power1 = GetCellValue(worksheet, row, 5),
                        Power2 = GetCellValue(worksheet, row, 6),
                        BoatInOut = GetCellValue(worksheet, row, 7),
                        MoveSpeed = GetCellValue(worksheet, row, 8),
                        UpDownSpeed = GetCellValue(worksheet, row, 9),
                        HeatTime = GetCellValue(worksheet, row, 10),
                        HeatTemp = GetCellValue(worksheet, row, 11),
                        PulseOn1 = GetCellValue(worksheet, row, 12),
                        PulseOff1 = GetCellValue(worksheet, row, 13),
                        PulseOn2 = GetCellValue(worksheet, row, 14),
                        PulseOff2 = GetCellValue(worksheet, row, 15),
                        CurrentVoltage = GetCellValue(worksheet, row, 16)
                    };

                    dataList.Add(rowData);
                }

                return dataList;
            }
            catch (Exception ex)
            {
                throw new Exception($"导入Excel失败: {ex.Message}");
            }
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col].Value;
            return cell?.ToString() ?? string.Empty;
        }
    }
} 