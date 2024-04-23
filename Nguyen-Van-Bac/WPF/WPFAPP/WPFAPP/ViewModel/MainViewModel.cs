using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WPFAPP.Extension.Excel;
using WPFAPP.Model;

namespace WPFAPP.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            ImportCommand = new RelayCommand(ImportInvoke);
            SelectedSheetCommand = new RelayCommand<string>(SwitchSheet);
            ExportCommand = new RelayCommand(ExportInvoke);
        }
        private string _selectedSheet;
        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                if (_selectedSheet != value)
                {
                    _selectedSheet = value;
                    NotifyChanged("SelectedSheet");
                    SelectedSheetCommand.Execute(null); // Gọi SelectedSheetCommand khi selected sheet thay đổi





                }
            }
        }

        public ObservableCollection<string> SheetNames { get; set; }
        public ObservableCollection<object> Data { get; set; }
       

        // Command to handle the selection of a new sheet
        public ICommand SelectedSheetCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ExcelFileInfo FileInfo { get; set; }
        public string FilePath { get; set; }
        private void ImportInvoke(object sender)
        {
            ChooseFileInvoke();
            if (!string.IsNullOrEmpty(FilePath))
            {
                var analyzer = new ExcelAnalyzer();
                FileInfo = analyzer.AnalyzerFile(FilePath);
                ObservableCollection<string> sheetNam = new ObservableCollection<string>(FileInfo.SheetNames);
                SheetNames = sheetNam;
                NotifyChanged("SheetNames");


            }
        }
        private void ExportInvoke(object sender)
        {
            // Hiển thị hộp thoại SaveFileDialog để người dùng chọn vị trí và tên file Excel mới
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    using (var package = new ExcelPackage())
                    {
                        // Tạo một worksheet mới
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

                        for (int i = 0; i < Data.Count; i++)
                        {


                            if (Data[i] is Student student)
                            {
                                worksheet.Cells[1, 1].Value = "ID";
                                worksheet.Cells[1, 2].Value = "Name";
                                worksheet.Cells[ 1, 3].Value = "Age";
                                worksheet.Cells[ 1, 4].Value = "Address";
                                worksheet.Cells[ 1, 5].Value = "TaxCode";
                                worksheet.Cells[1, 6].Value = "Imcome";
                                worksheet.Cells[ 1, 7].Value = "School";
                                worksheet.Cells[ 1, 8].Value = "Class";

                                worksheet.Cells[i + 2, 1].Value = student.ID;
                                worksheet.Cells[i + 2, 2].Value = student.Name;
                                worksheet.Cells[i + 2, 3].Value = student.Age;
                                worksheet.Cells[i + 2, 4].Value = student.Address;
                                worksheet.Cells[i + 2, 5].Value = student.TaxCode;
                                worksheet.Cells[i + 2, 6].Value = student.Imcome;
                                worksheet.Cells[i + 2, 7].Value = student.School;
                                worksheet.Cells[i + 2, 8].Value = student.Class;
                            }
                            else if (Data[i] is Teacher teacher)
                            {
                                worksheet.Cells[1, 1].Value = "ID";
                                worksheet.Cells[1, 2].Value = "Name";
                                worksheet.Cells[1, 3].Value = "Age";
                                worksheet.Cells[1, 4].Value = "Address";
                                worksheet.Cells[1, 5].Value = "TaxCode";
                                worksheet.Cells[1, 6].Value = "Imcome";
                                worksheet.Cells[1, 7].Value = "School";
                                worksheet.Cells[i + 1, 1].Value = teacher.ID;
                                worksheet.Cells[i + 1, 2].Value = teacher.Name;
                                worksheet.Cells[i + 1, 3].Value = teacher.Age;
                                worksheet.Cells[i + 1, 4].Value = teacher.Address;
                                worksheet.Cells[i + 1, 5].Value = teacher.TaxCode;
                                worksheet.Cells[i + 1, 6].Value = teacher.Imcome;
                                worksheet.Cells[i + 1, 7].Value = teacher.School;
                            }
                            else if (Data[i] is Employee employee)
                            {
                                worksheet.Cells[1, 1].Value = "ID";
                                worksheet.Cells[1, 2].Value = "Name";
                                worksheet.Cells[1, 3].Value = "Age";
                                worksheet.Cells[1, 4].Value = "Address";
                                worksheet.Cells[1, 5].Value = "TaxCode";
                                worksheet.Cells[1, 6].Value = "Imcome";
                                worksheet.Cells[i + 1, 1].Value = employee.ID;
                                worksheet.Cells[i + 1, 2].Value = employee.Name;
                                worksheet.Cells[i + 1, 3].Value = employee.Age;
                                worksheet.Cells[i + 1, 4].Value = employee.Address;
                                worksheet.Cells[i + 1, 5].Value = employee.TaxCode;
                                worksheet.Cells[i + 1, 6].Value = employee.Imcome;
                            }

                        }

                        package.SaveAs(new FileInfo(filePath));
                    }

                    MessageBox.Show("Dữ liệu đã được xuất thành công vào " + filePath, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xuất dữ liệu không thành công. Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChooseFileInvoke()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }
        private void SwitchSheet(string selectedSheet)
        {
            switch (SelectedSheet)
            {
                case "Student":
                    Data = ExcelReader.ReadStudents(FilePath);
                    NotifyChanged("Data");
                    break;
                case "Teacher":
                    Data = ExcelReader.ReadTeachers(FilePath);
                    NotifyChanged("Data");
                    break;
                case "Employee":
                    Data = ExcelReader.ReadEmployees(FilePath);
                    NotifyChanged("Data");
                    break;
                default:
                    break;
            }
        }
    }

}
