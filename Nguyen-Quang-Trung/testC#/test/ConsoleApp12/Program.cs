using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

using Excel = Microsoft.Office.Interop.Excel; // dể gọi thuộc tính và phương thức của thư viện ngắn gọn hơn

namespace ConsoleApp12
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ////Đọc và ghi file excel
            //Console.WriteLine("mo file excel");
            //// chạy file Excel theo đường dẫn
            //Excel.Application xlApp = new Excel.Application();
            //Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(@"E:\trainC#\Training\Nguyen-Quang-Trung\testC#\test\ConsoleApp12\bin\Debug\vidu1.xlsx");
            //// Lấy Sheet 1
            //Excel.Worksheet xlWorksheet = (Excel.Worksheet)xlWorkbook.Sheets.get_Item(1);
            //// Lấy phạm vi dữ liệu
            //Excel.Range xlRange = xlWorksheet.UsedRange;
            //// Tạo mảng lưu trữ dữ liệu
            //object[,] valueArray = (object[,])xlRange.get_Value(Excel.XlRangeValueDataType.xlRangeValueDefault);

            //// Hiển thị nọi dung
            //for (int row = 1; row <= xlWorksheet.UsedRange.Rows.Count; ++row)//đọc row hiện có trong Excel
            //{
            //    for (int colum = 1; colum <= xlWorksheet.UsedRange.Columns.Count; ++colum)//đọc colum trong Excel
            //    {
            //        String giatri = valueArray[row, colum].ToString();//Giá trị = valueArray[dòng, cột]; ToString() là để chuyển giá trị thành dạng String
            //        Console.WriteLine(giatri);
            //    }
            //}
            //Console.ReadLine();
            //// Đóng Workbook.
            //xlWorkbook.Close(false);
            //// Đóng application.
            //xlApp.Quit();
            //// Khử hết đối tượng
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlWorkbook);
            //System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);

            // tạo file excel
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;

            xlWorkBook = xlApp.Workbooks.Add();
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            xlWorkSheet.Cells[1, 1] = "ID";
            xlWorkSheet.Cells[1, 2] = "Name";
            xlWorkSheet.Cells[2, 1] = "1";
            xlWorkSheet.Cells[2, 2] = "One";
            xlWorkSheet.Cells[3, 1] = "2";
            xlWorkSheet.Cells[3, 2] = "Two";

            xlWorkBook.SaveAs(@"E:\trainC#\Training\Nguyen-Quang-Trung\testC#\test\ConsoleApp12\bin\Debug\vidu2.xlsx");
            xlWorkBook.Close(true);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
        }
    }
}