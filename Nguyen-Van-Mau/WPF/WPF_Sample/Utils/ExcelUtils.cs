using System;
using System.Data;
using System.Windows;
using ClosedXML.Excel;

namespace WPF_Sample.Utils
{
    /// <summary>
    /// The utility support for interactive with excel
    /// </summary>
    internal class ExcelUtils
    {
        /// <summary>
        /// Export data to  excel file
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool Export(DataTable table, string filePath)
        {
            bool isSuccess = false;
            try
            {
                XLWorkbook wb = new XLWorkbook();
                IXLWorksheet ws = wb.Worksheets.Add(table.TableName);
                ws.Cell("A1").Value = table.TableName;
                int columnsCount = table.Columns.Count;
                IXLRange titleRange = ws.Range(1, 1, 1, columnsCount);
                titleRange.FirstRow().Merge();
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRange.Style.Fill.BackgroundColor = XLColor.Green;
                ws.Cell("A2").InsertTable(table);

                ws.Columns().AdjustToContents();
                wb.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                // throw;
                MessageBox.Show(ex.Message);
            }

            return isSuccess;
        }
    }
}