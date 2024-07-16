using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class ExcelHelper
    {
        #region Get a list of excel sheet names

        public static List<string> GetSheetsName(string filePath)
        {
            List<string> Sheets = new List<string>();
            if (File.Exists(filePath))
            {
                // Khởi tạo gói Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheets = package.Workbook.Worksheets;
                    foreach (var worksheet in worksheets)
                    {
                        Sheets.Add(worksheet.Name);
                    }
                }
            }
            return Sheets;
        }

        #endregion Get a list of excel sheet names

        #region Get a list of excel sheet data

        public static List<T> GetData<T>(string filePath, string sheetName)
        {
            List<T> objects = new List<T>();

            if (File.Exists(filePath))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[sheetName];
                    if (worksheet != null)
                    {
                        if (worksheet.Dimension != null)
                        {
                            int rowCount = worksheet.Dimension.End.Row;
                            int colCount = worksheet.Dimension.End.Column;
                            int colHeader = 0;
                            int rowHeader = 0;
                            bool find = false;

                            for (int row = 1; row <= rowCount; row++)
                            {
                                for (int col = 1; col <= colCount; col++)
                                {
                                    if (!string.IsNullOrEmpty(worksheet.Cells[row, col].Value?.ToString()))
                                    {
                                        colHeader = col;
                                        rowHeader = row;
                                        find = true;
                                        break;
                                    }
                                }
                                if (find) break;
                            }

                            if (colHeader != 0 || rowHeader != 0)
                            {
                                for (int row = rowHeader + 1; row <= rowCount; row++)
                                {
                                    T obj = Activator.CreateInstance<T>();

                                    for (int col = colHeader; col <= colCount; col++)
                                    {
                                        string propertyName = worksheet.Cells[rowHeader, col].Value?.ToString();
                                        if (propertyName == null) continue;

                                        // Sử dụng reflection để gán giá trị vào thuộc tính của đối tượng T
                                        var property = typeof(T).GetProperty(propertyName);
                                        try
                                        {
                                            if (property != null)
                                            {
                                                string cellValue = worksheet.Cells[row, col].Value?.ToString();
                                                property.SetValue(obj, Convert.ChangeType(cellValue, property.PropertyType));
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            continue; // Nếu gán giá trị cho thuộc tính thất bại chuyển sang ô tiếp theo
                                        }
                                    }

                                    objects.Add(obj);
                                }
                            }
                        }
                    }
                }
            }
            return objects;
        }

        #endregion Get a list of excel sheet data

        #region Export sheet data to excel file

        public static bool ExportData(string filePath, Dictionary<string, List<Object>> Data)
        {
            try
            {
                // Tạo file Excel mới
                using (var package = new ExcelPackage())
                {
                    foreach (var data in Data)
                    {
                        var worksheet = package.Workbook.Worksheets.Add(data.Key);

                        if (data.Value == null || !data.Value.Any())
                            continue;
                        // Lấy danh sách các thuộc tính từ đối tượng đầu tiên
                        var properties = data.Value.First().GetType().GetProperties().Where(p => p.Name != "Id").ToList();
                        // Ghi tiêu đề cột
                        for (int i = 0; i < properties.Count(); i++)
                        {
                            if (properties[i].Name == "Id") continue;
                            worksheet.Cells[1, i + 1].Value = properties[i].Name;
                        }

                        // Ghi dữ liệu từ danh sách
                        int row = 2;
                        foreach (var item in data.Value)
                        {
                            for (int i = 0; i < properties.Count(); i++)
                            {
                                var value = properties[i].GetValue(item);
                                worksheet.Cells[row, i + 1].Value = value;
                            }
                            row++;
                        }
                        FileInfo fi = new FileInfo(filePath);
                        package.SaveAs(fi);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Export sheet data to excel file
    }
}