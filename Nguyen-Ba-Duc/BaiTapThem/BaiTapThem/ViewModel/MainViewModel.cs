using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BaiTapThem.View;
using Microsoft.Win32;
using OfficeOpenXml;

namespace BaiTapThem.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand ExportCommnand { get; set; }

        private string filePath = "";

        public MainViewModel()
        {
            //NhanVienView nvView = new NhanVienView();
            //nvView.ShowDialog();

            //PhongBanView pbView = new PhongBanView();
            //pbView.ShowDialog();

            ExportCommnand = new RelayCommand(ExportDemo);
        }

        private void ExportDemo(object p)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("The path is invalid");
                return;
            }
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Demo");

                for (int col = 1; col < 1000; col++)
                {
                    for (int row = 1; row < 10000; row++)
                    {
                        worksheet.Cells[row, col].Value = "Just Test";
                    }
                }

                package.SaveAs(new FileInfo(filePath));
            }
            MessageBox.Show("Xuất excel thành công!");
        }
    }
}