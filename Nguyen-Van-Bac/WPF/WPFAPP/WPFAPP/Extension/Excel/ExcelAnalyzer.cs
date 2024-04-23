using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WPFAPP.Extension.Excel
{
    public class ExcelAnalyzer
    {
        public ExcelFileInfo AnalyzerFile(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var info = new ExcelFileInfo
                {
                    SheetNames = package.Workbook.Worksheets.Select(ws => ws.Name).ToList()
                };
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    info.ColumnCounts.Add(sheet.Dimension.Columns);
                }
                return info;
            }
        }
    }
    public class ExcelFileInfo
    {
        public List<string> SheetNames { get; set; } = new List<string>();
        public List<int> ColumnCounts { get; set; } = new List<int>();
    }
}
