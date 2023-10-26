using APP_WPF_MVVM.Model;
using APP_WPF_MVVM.View;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APP_WPF_MVVM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        //SelectedIndex cbo
        public OpenFileDialog ofd1;
        private int _index;
        public int Index 
        { 
            get { return _index; }
            set
            {
                if (value != _index)
                {
                    _index = value;
                    ShowSheetView(value);
                   // NotifyPropertyChanged();
                }
            }
        }
        //ObjectList
        private ObservableCollection<object> _userList;
        public ObservableCollection<object> UserList
        {
            get { return _userList; }
            set { _userList = value; NotifyPropertyChanged(); }
        }
        // StudentList
        private ObservableCollection<Students> _studentList;
        public ObservableCollection<Students> StudentList
        {
            get { return _studentList; }
            set { _studentList = value; NotifyPropertyChanged(); }
        }
        // TeacherList
        private ObservableCollection<Teachers> _teacherList;
        public ObservableCollection<Teachers> TeacherList
        {
            get { return _teacherList; }
            set { _teacherList = value; NotifyPropertyChanged(); }
        }
        // EmployeeList
        private ObservableCollection<Employees> _employeeList;
        public ObservableCollection<Employees> EmployeeList
        {
            get { return _employeeList; }
            set { _employeeList = value; NotifyPropertyChanged(); }
        }
        //SheetNames - Lưu tên các sheets có trong file excel
        public ObservableCollection<string> _sheetNames;
        public ObservableCollection<string> SheetNames
        {
            get { return _sheetNames; }
            set { _sheetNames = value; NotifyPropertyChanged(); }
        }
        //Khai báo thuộc tính kiểu Icommand 
        public ICommand ImportCommand { get; }//dung cho nut Import
        public ICommand ExportCommand { get; }//dung cho nut Export

        public MainViewModel()
        {
            ImportCommand = new RelayCommand(ImportFull);
            ExportCommand = new RelayCommand(Export);

        }
        private OpenFileDialog ofd()//Tao 
        {
            OpenFileDialog x = new OpenFileDialog();
            x.ShowDialog();
            return x;
        }
        private void ShowSheetView(int in1) //Hien thi sheet ra view
        {
            //MessageBox.Show(in1.ToString());
            if (in1 == 0)
            {
                ShowStudents(in1, ofd1);
                //Index = 0;
            }
            if (in1 == 1)
            {
                ShowTeachers(in1, ofd1);
                //Index = 1;
            }
            if (in1 == 2)
            {
                ShowEmployees(in1, ofd1);
                //Index = 2;
            }
           // MessageBox.Show(in1.ToString());
        }
        private void Import(int index) // hien thi du lieu ra datagrid
        {
            MessageBox.Show("Chọn file cần mở", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            ofd1 = new OpenFileDialog();
            ofd1 = ofd();
            ShowSheetView(index);
        }
        private void ImportFull ()
        {
            Import(Index);
        }
        private void ShowComboBox(ExcelPackage package) //Lấy ra tên của các sheet có trong excel va cho vao Sheetnames
        {
            SheetNames = new ObservableCollection<string>();
            // Lấy ra tên của các sheet có trong excel
            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                SheetNames.Add(worksheet.Name);
            }
        }
        private void ShowStudents(int index, OpenFileDialog ofd)//Hien thi du lieu cua sheet student ra datagrid
        {
            MessageBox.Show("ShowStudent");
            try
            {
                //OpenFileDialog ofd = new OpenFileDialog();
                // ofd.ShowDialog();
                //  ofd.FileName = @"Documenten";

                // tạo ra danh sách UserInfo rỗng để hứng dữ liệu.
                StudentList = new ObservableCollection<Students>();
                UserList = new ObservableCollection<object>();
                // mở file excel
                var package = new ExcelPackage(new FileInfo(ofd.FileName));
                
                // lấy ra sheet thứ index để thao tác
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelWorksheet workSheet = package.Workbook.Worksheets[index];

                //lấy ra ten cac sheet de hien thi ra cbo
                ShowComboBox(package);
                // duyệt tuần tự từ dòng thứ 2 đến dòng cuối cùng của file. lưu ý file excel bắt đầu từ số 1 không phải số 0
                for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        // biến j biểu thị cho một column trong file
                        int j = 1;

                        // lấy ra cột họ tên tương ứng giá trị tại vị trí [i, 1]. i lần đầu là 2
                        // tăng j lên 1 đơn vị sau khi thực hiện xong câu lệnh
                        string id = workSheet.Cells[i, j++].Value.ToString();
                        string name = workSheet.Cells[i, j++].Value.ToString();
                        int age = int.Parse(workSheet.Cells[i, j++].Value.ToString());
                        string address = workSheet.Cells[i, j++].Value.ToString();
                        string class1 = workSheet.Cells[i, j++].Value.ToString();
                        string school = workSheet.Cells[i, j++].Value.ToString();
                        // tạo Student từ dữ liệu đã lấy được ở sheet Student
                        Students user = new Students()
                        {
                            Id = id,
                            Name = name,
                            Age = age,
                            Address = address,
                            Class = class1,
                            School = school
                        };

                        // add du lieu o sheet student vào danh sách StudentList
                        UserList.Add(user);
                        StudentList.Add(user);
                    }
                    catch (Exception exe)
                    {

                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Co loi xay ra", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowTeachers(int index, OpenFileDialog ofd)//Hien thi du lieu cua sheet teacher ra datagrid
        {
            MessageBox.Show("ShowTeacher");
            try
            {
               // OpenFileDialog ofd = new OpenFileDialog();
               // ofd.ShowDialog();
                //  ofd.FileName = @"Documenten";

                // tạo ra danh sách UserInfo rỗng để hứng dữ liệu.
                TeacherList = new ObservableCollection<Teachers>();
                UserList = new ObservableCollection<object>();
                // mở file excel
                var package = new ExcelPackage(new FileInfo(ofd.FileName));

                // lấy ra sheet thứ index để thao tác
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelWorksheet workSheet = package.Workbook.Worksheets[index];
                ShowComboBox(package);
                //lấy ra ten cac sheet de hien thi ra cbo
                // duyệt tuần tự từ dòng thứ 2 đến dòng cuối cùng của file. lưu ý file excel bắt đầu từ số 1 không phải số 0
                for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        // biến j biểu thị cho một column trong file
                        int j = 1;

                        // lấy ra cột họ tên tương ứng giá trị tại vị trí [i, 1]. i lần đầu là 2
                        // tăng j lên 1 đơn vị sau khi thực hiện xong câu lệnh
                        string id = workSheet.Cells[i, j++].Value.ToString();
                        string name = workSheet.Cells[i, j++].Value.ToString();
                        int age = int.Parse(workSheet.Cells[i, j++].Value.ToString());
                        string address = workSheet.Cells[i, j++].Value.ToString();
                        string school = workSheet.Cells[i, j++].Value.ToString();
                        // tạo Student từ dữ liệu đã lấy được ở sheet Student
                        Teachers user = new Teachers()
                        {
                            Id = id,
                            Name = name,
                            Age = age,
                            Address = address,
                            School = school
                        };

                        // add du lieu o sheet student vào danh sách StudentList
                        UserList.Add(user);
                        TeacherList.Add(user);
                    }
                    catch (Exception exe)
                    {

                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Co loi xay ra", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowEmployees(int index, OpenFileDialog ofd)//Hien thi du lieu cua sheet employee ra datagrid
        {
            MessageBox.Show("ShowEmloyee");
            try
            {
               // OpenFileDialog ofd = new OpenFileDialog();
               // ofd.ShowDialog();
                //  ofd.FileName = @"Documenten";

                // tạo ra danh sách UserInfo rỗng để hứng dữ liệu.
                EmployeeList = new ObservableCollection<Employees>();
                UserList = new ObservableCollection<object>();
                // mở file excel
                var package = new ExcelPackage(new FileInfo(ofd.FileName));

                // lấy ra sheet thứ index để thao tác
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelWorksheet workSheet = package.Workbook.Worksheets[index];
                //lấy ra ten cac sheet de hien thi ra cbo
                ShowComboBox(package);
                // duyệt tuần tự từ dòng thứ 2 đến dòng cuối cùng của file. lưu ý file excel bắt đầu từ số 1 không phải số 0
                for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        // biến j biểu thị cho một column trong file
                        int j = 1;

                        // lấy ra cột họ tên tương ứng giá trị tại vị trí [i, 1]. i lần đầu là 2
                        // tăng j lên 1 đơn vị sau khi thực hiện xong câu lệnh
                        string id = workSheet.Cells[i, j++].Value.ToString();
                        string name = workSheet.Cells[i, j++].Value.ToString();
                        int age = int.Parse(workSheet.Cells[i, j++].Value.ToString());
                        string address = workSheet.Cells[i, j++].Value.ToString();
                        double income = double.Parse(workSheet.Cells[i, j++].Value.ToString());
                        double taxtoe = double.Parse(workSheet.Cells[i, j++].Value.ToString());
                        // tạo Employee từ dữ liệu đã lấy được ở sheet Employee
                        Employees user = new Employees()
                        {
                            Id = id,
                            Name = name,
                            Age = age,
                            Address = address,
                            Income = income,
                            Taxcoe = taxtoe
                        };

                        // add du lieu o sheet student vào danh sách EmployeetList
                        UserList.Add(user);
                        EmployeeList.Add(user);
                    }
                    catch (Exception exe)
                    {

                    }
                }
                

            }
            catch (Exception ee)
            {
                MessageBox.Show("Co loi xay ra!", "Thong bao", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TienExport()//duyet qua tat ca sheet truoc khi xuat
        {
            ShowStudents(0, ofd1);
            ShowTeachers(1, ofd1);
            ShowEmployees(2, ofd1);
        }
        private void ExportStudents(ExcelPackage p)// export sheet Students
        {
            // lấy sheet ra để thao tác
            ExcelWorksheet ws1 = p.Workbook.Worksheets[0];
            // đặt tên cho sheet
            ws1.Name = "Students";
            // fontsize mặc định cho cả sheet
            ws1.Cells.Style.Font.Size = 11;
            // font family mặc định cho cả sheet
            ws1.Cells.Style.Font.Name = "Calibri";
            // Tạo danh sách các column header
            string[] arrColumnHeader1 = {
                                                "ID",
                                                "Name",
                                                "AGE",
                                                "ADDRESS",
                                                "CLASS",
                                                "SCHOOL"
                     };
            // lấy ra số lượng cột cần dùng dựa vào số lượng header
            var countColHeader1 = arrColumnHeader1.Count();
            int colIndex1 = 1;
            int rowIndex1 = 1;
            foreach (var item in arrColumnHeader1)
            {
                var cell = ws1.Cells[rowIndex1, colIndex1];

                //set màu thành gray
                var fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                //căn chỉnh các border
                var border = cell.Style.Border;
                border.Bottom.Style =
                    border.Top.Style =
                    border.Left.Style =
                    border.Right.Style = ExcelBorderStyle.Thin;

                //gán giá trị
                cell.Value = item;

                colIndex1++;
            }
            // với mỗi item trong danh sách sẽ ghi trên 1 dòng
            foreach (var item in StudentList)
            {
                // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0
                colIndex1 = 1;

                // rowIndex tương ứng từng dòng dữ liệu
                rowIndex1++;

                //gán giá trị cho từng cell                      
                ws1.Cells[rowIndex1, colIndex1++].Value = item.Id;
                ws1.Cells[rowIndex1, colIndex1++].Value = item.Name;
                ws1.Cells[rowIndex1, colIndex1++].Value = item.Age;
                ws1.Cells[rowIndex1, colIndex1++].Value = item.Address;
                ws1.Cells[rowIndex1, colIndex1++].Value = item.Class;
                ws1.Cells[rowIndex1, colIndex1++].Value = item.School;
            }
        }
        private void ExportTeachers(ExcelPackage p)// export sheet Teachers
        {
            // lấy sheet ra để thao tác
            ExcelWorksheet ws2 = p.Workbook.Worksheets[1];
            // đặt tên cho sheet
            ws2.Name = "Teachers";
            // fontsize mặc định cho cả sheet
            ws2.Cells.Style.Font.Size = 11;
            // font family mặc định cho cả sheet
            ws2.Cells.Style.Font.Name = "Calibri";
            string[] arrColumnHeader2 = {
                                                    "ID",
                                                    "Name",
                                                    "AGE",
                                                    "ADDRESS",
                                                    "SCHOOL"
                     };
            var countColHeader2 = arrColumnHeader2.Count();
            int colIndex2 = 1;
            int rowIndex2 = 1;
            //tạo các header từ column header đã tạo từ bên trên
            foreach (var item in arrColumnHeader2)
            {
                var cell = ws2.Cells[rowIndex2, colIndex2];

                //set màu thành gray
                var fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                //căn chỉnh các border
                var border = cell.Style.Border;
                border.Bottom.Style =
                    border.Top.Style =
                    border.Left.Style =
                    border.Right.Style = ExcelBorderStyle.Thin;

                //gán giá trị
                cell.Value = item;
                colIndex2++;
            }
            foreach (var item in TeacherList)
            {
                // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0
                colIndex2 = 1;

                // rowIndex tương ứng từng dòng dữ liệu
                rowIndex2++;

                //gán giá trị cho từng cell                      
                ws2.Cells[rowIndex2, colIndex2++].Value = item.Id;
                ws2.Cells[rowIndex2, colIndex2++].Value = item.Name;
                ws2.Cells[rowIndex2, colIndex2++].Value = item.Age;
                ws2.Cells[rowIndex2, colIndex2++].Value = item.Address;
                ws2.Cells[rowIndex2, colIndex2++].Value = item.School;
            }
        }
        private void ExportEmployees(ExcelPackage p)// export sheet Eployees
        {
            // lấy sheet vừa add ra để thao tác
            ExcelWorksheet ws3 = p.Workbook.Worksheets[2];
            // đặt tên cho sheet
            ws3.Name = "Employees";
            // fontsize mặc định cho cả sheet
            ws3.Cells.Style.Font.Size = 11;
            // font family mặc định cho cả sheet
            ws3.Cells.Style.Font.Name = "Calibri";
            string[] arrColumnHeader3 = {
                                                        "ID",
                                                        "Name",
                                                        "AGE",
                                                        "ADDRESS",
                                                        "INCOME",
                                                        "TAXCOE"
                         };


            var countColHeader3 = arrColumnHeader3.Count();
            int colIndex3 = 1;
            int rowIndex3 = 1;


            //tạo các header từ column header đã tạo từ bên trên
            foreach (var item in arrColumnHeader3)
            {
                var cell = ws3.Cells[rowIndex3, colIndex3];

                //set màu thành gray
                var fill = cell.Style.Fill;
                fill.PatternType = ExcelFillStyle.Solid;
                fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                //căn chỉnh các border
                var border = cell.Style.Border;
                border.Bottom.Style =
                    border.Top.Style =
                    border.Left.Style =
                    border.Right.Style = ExcelBorderStyle.Thin;

                //gán giá trị
                cell.Value = item;
                colIndex3++;
            }
                foreach (var item in EmployeeList)
                {
                    // bắt đầu ghi từ cột 1. Excel bắt đầu từ 1 không phải từ 0
                    colIndex3 = 1;

                    // rowIndex tương ứng từng dòng dữ liệu
                    rowIndex3++;

                    //gán giá trị cho từng cell                      
                    ws3.Cells[rowIndex3, colIndex3++].Value = item.Id;
                    ws3.Cells[rowIndex3, colIndex3++].Value = item.Name;
                    ws3.Cells[rowIndex3, colIndex3++].Value = item.Age;
                    ws3.Cells[rowIndex3, colIndex3++].Value = item.Address;
                    ws3.Cells[rowIndex3, colIndex3++].Value = item.Income;
                    ws3.Cells[rowIndex3, colIndex3++].Value = item.Taxcoe;
                }
        }
        private void Export()//export ra file excel
        {
            MessageBox.Show("Da vao export");
           
            string filePath = "";
            // tạo SaveFileDialog để lưu file excel
            SaveFileDialog dialog = new SaveFileDialog();

            // chỉ lọc ra các file có định dạng Excel
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";

            // Nếu mở file và chọn nơi lưu file thành công sẽ lưu đường dẫn lại dùng
            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            // nếu đường dẫn null hoặc rỗng thì báo không hợp lệ và return hàm
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn không hợp lệ");
                return;
            }

            try
             {
                TienExport();
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage p = new ExcelPackage())
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "DucLe";
                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "Báo cáo thống kê";
                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("Students");
                    p.Workbook.Worksheets.Add("Teachers");
                    p.Workbook.Worksheets.Add("Employees");
                    //Export
                    ExportStudents(p);
                    ExportTeachers(p);
                    ExportEmployees(p);

                    //Lưu file lại
                    Byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");

            }
            catch (Exception EE)
            {
                MessageBox.Show("Có lỗi khi lưu file!");
            }
        }
    }
}
