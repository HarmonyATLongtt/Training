using Ex_WPF.ModelView;
using ExcelDataReader;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Ex_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            //Đọc data từ file excecl

            //using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            //{
            //    using (var reader = ExcelReaderFactory.CreateReader(stream))
            //    {
            //        var dataSet = reader.AsDataSet();
            //        var dataTable = dataSet.Tables["Student"];
            //        var bindingList = new BindingList<Student>();

            //      for (int indexRow = 1; indexRow<dataTable.Rows.Count; indexRow++)
            //      {
            //            DataRow row = dataTable.Rows[indexRow];
            //            Student student = new Student();
            //            student.ID = row[0].ToString();
            //            student.Name = row[1].ToString();
            //            student.Age = int.Parse(row[2].ToString());
            //            student.Address = row[3].ToString();
            //            student.TaxFactor = double.Parse(row[4].ToString());

            //            bindingList.Add(student);
            //            studentsDataGrid.ItemsSource = bindingList;
            //        }

            //    }
            //}

            // Set data context cho data grid
            DataContext = new BaseViewModel();
        }

        private void btnClickImport(object sender, RoutedEventArgs e)
        {
            // Khởi tạo OpenFileDialog để lựa chọn file excel
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog.ShowDialog() == true)
            {
                // Lấy đường dẫn của file excel đã chọn
                string filePath = openFileDialog.FileName;

                // Khởi tạo FileStream để đọc file excel
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Khởi tạo ExcelDataReader để đọc dữ liệu từ file excel
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Đọc dữ liệu từ sheet Student vào DataTable
                        reader.Read();
                        var dt = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        }).Tables["Student"];

                        // Binding dữ liệu lên ListView
                        studentsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
        }
        private void btnClickExport(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx;*.xls;*.xlsm";

            if (saveFileDialog.ShowDialog() == true)
            {
                // Lấy đường dẫn để lưu file excel
                string filePath = saveFileDialog.FileName;

                // Khởi tạo FileStream để lưu file excel
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    // Khởi tạo ExcelPackage để tạo file excel
                    using (var excelPackage = new ExcelPackage())
                    {
                        // Tạo một worksheet với tên là Student
                        var worksheet = excelPackage.Workbook.Worksheets.Add("Student");

                        // Lấy dữ liệu từ ListView và đưa vào DataTable
                        DataTable dt = ((DataView)studentsDataGrid.ItemsSource).ToTable();

                        // Lưu dữ liệu vào worksheet
                        worksheet.Cells.LoadFromDataTable(dt, true);

                        // Lưu file excel
                        excelPackage.SaveAs(stream);
                    }
                }

                MessageBox.Show("Export successful!");
            }
        }
        private void btnClickClear(object sender, RoutedEventArgs e)
        {
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Hủy bỏ và đóng màn hình view
                Close();
            }
        }
    }

}
