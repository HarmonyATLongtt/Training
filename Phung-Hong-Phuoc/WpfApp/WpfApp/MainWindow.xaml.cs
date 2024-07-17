using Microsoft.Win32;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System;
using System.ComponentModel;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private ExcelPackage package;
        private Dictionary<string, List<People>> sheetData;

        public class People : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public int ID { get; set; }
            public string Adr { get; set; }
            public double TaxCoe { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            sheetData = new Dictionary<string, List<People>>();
        }

        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel files(*.xlsx)| *.xlsx";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string filepath = ofd.FileName;
                    LoadDataSheet(filepath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing file: " + ex.Message);
                }
            }
        }

        private void LoadDataSheet(string filename)
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                cbSheetNames.Items.Clear();
                package = new ExcelPackage(new FileInfo(filename));
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    cbSheetNames.Items.Add(worksheet.Name);
                }
                if (cbSheetNames.Items.Count > 0)
                {
                    cbSheetNames.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data sheet: " + ex.Message);
            }
        }

        private void cbSheetNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (package == null || cbSheetNames.SelectedIndex < 0)
                return;

            var selectedSheetName = cbSheetNames.SelectedItem.ToString();
            var sheet = package.Workbook.Worksheets[selectedSheetName];
            List<People> peopleList = new List<People>();

            for (var rowNumber = 2; rowNumber <= sheet.Dimension.End.Row; rowNumber++)
            {
                var people = new People();
                people.Name = sheet.Cells[rowNumber, 1].Text;

                int age;
                if (int.TryParse(sheet.Cells[rowNumber, 2].Text, out age))
                {
                    people.Age = age;
                }

                int id;
                if (int.TryParse(sheet.Cells[rowNumber, 3].Text, out id))
                {
                    people.ID = id;
                }

                people.Adr = sheet.Cells[rowNumber, 4].Text;

                double taxCoe;
                if (double.TryParse(sheet.Cells[rowNumber, 5].Text, out taxCoe))
                {
                    people.TaxCoe = taxCoe;
                }

                peopleList.Add(people);
            }

            sheetData[selectedSheetName] = peopleList;
            dataGrid.ItemsSource = peopleList;
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";

            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn báo cáo không hợp lệ");
                return;
            }

            try
            {
                using (ExcelPackage p = new ExcelPackage())
                {
                    p.Workbook.Properties.Author = "Bai tap wpf";
                    p.Workbook.Properties.Title = "Báo cáo bài tập";
                    foreach (var sheetName in sheetData.Keys)
                    {
                        p.Workbook.Worksheets.Add(sheetName);
                        ExcelWorksheet ws = p.Workbook.Worksheets[sheetName];

                        ws.Name = sheetName;
                        ws.Cells.Style.Font.Size = 11;
                        ws.Cells.Style.Font.Name = "Calibri";

                        string[] arrColumnHeader = { "Họ tên", "Tuổi", "ID", "Địa chỉ", "Hệ số thuế" };
                        var countColHeader = arrColumnHeader.Length;

                        ws.Cells[1, 1].Value = "Thống kê thông tin";
                        ws.Cells[1, 1, 1, countColHeader].Merge = true;
                        ws.Cells[1, 1, 1, countColHeader].Style.Font.Bold = true;
                        ws.Cells[1, 1, 1, countColHeader].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        int colIndex = 1;
                        int rowIndex = 2;

                        foreach (var item in arrColumnHeader)
                        {
                            var cell = ws.Cells[rowIndex, colIndex];
                            var fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                            var border = cell.Style.Border;
                            border.Bottom.Style =
                                border.Top.Style =
                                border.Left.Style =
                                border.Right.Style = ExcelBorderStyle.Thin;

                            cell.Value = item;
                            colIndex++;
                        }

                        List<People> peopleList = sheetData[sheetName];

                        foreach (var person in peopleList)
                        {
                            colIndex = 1;
                            rowIndex++;
                            ws.Cells[rowIndex, colIndex++].Value = person.Name;
                            ws.Cells[rowIndex, colIndex++].Value = person.Age;
                            ws.Cells[rowIndex, colIndex++].Value = person.ID;
                            ws.Cells[rowIndex, colIndex++].Value = person.Adr;
                            ws.Cells[rowIndex, colIndex++].Value = person.TaxCoe;
                        }
                    }

                    Byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi lưu file!");
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (package != null)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files(*.xlsx)| *.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    package.SaveAs(new FileInfo(saveDialog.FileName));
                }
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                People editedPerson = e.Row.Item as People;

                if (editedPerson != null && cbSheetNames.SelectedItem != null)
                {
                    try
                    {
                        var selectedSheetName = cbSheetNames.SelectedItem.ToString();
                        var sheet = package.Workbook.Worksheets[selectedSheetName];

                        for (var rowNumber = 2; rowNumber <= sheet.Dimension.End.Row; rowNumber++)
                        {
                            if (sheet.Cells[rowNumber, 3].Text == editedPerson.ID.ToString())
                            {
                                sheet.Cells[rowNumber, 1].Value = editedPerson.Name;
                                sheet.Cells[rowNumber, 2].Value = editedPerson.Age;
                                sheet.Cells[rowNumber, 3].Value = editedPerson.ID;
                                sheet.Cells[rowNumber, 4].Value = editedPerson.Adr;
                                sheet.Cells[rowNumber, 5].Value = editedPerson.TaxCoe;
                                break;
                            }
                        }
                        package.Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error editing data: " + ex.Message);
                    }
                }
            }
        }
    }
}