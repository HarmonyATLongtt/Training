using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpen(object sender, RoutedEventArgs e)
        {
            List<UserInfo> UserList = new List<UserInfo>();
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                //tạo obj excelPackage mở từ file Openfile -> đọc/ ghi DL
                var package = new ExcelPackage(new FileInfo("./Openfile.xlsx"));

                // Lấy Sheet đầu tiên
                ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                //duyệt từng hàng từ hàng t2 (hàng đầu chưa tiêu đề)
                for (int i = worksheet.Dimension.Start.Row + 1; i <= worksheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        int j = 1;
                        int id = int.Parse(worksheet.Cells[i, j++].Value.ToString());
                        string name = worksheet.Cells[i, j++].Value.ToString();
                        string address = worksheet.Cells[i, j++].Value.ToString();

                        //lấy màu của ô
                        var cell = worksheet.Cells[i, 1];
                        Color backgroundColor = cell.Style.Fill.BackgroundColor.Color;

                        UserInfo user = new UserInfo();
                        {
                            user.Stt = id;
                            user.Name = name;
                            user.Address = address;
                            user.BackgroundColor = backgroundColor;
                        };

                        UserList.Add(user);
                    }
                    catch (Exception exe)
                    {
                        MessageBox.Show("Error!: " + exe.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error1!" + ex.Message);
            }
            dtgExcel.ItemsSource = UserList;
            dtgExcel.RowStyle = new Style(typeof(DataGridRow))
            {
                Setters =
        {
            new Setter(DataGridRow.BackgroundProperty, new Binding("BackgroundColor"))
        }
            };
        }

        private void btnSave(object sender, RoutedEventArgs e)
        {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var package = new ExcelPackage(new FileInfo("./Openfile.xlsx"));

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                    List<UserInfo> UserList = (List<UserInfo>)dtgExcel.ItemsSource;

                    for (int i = 0; i < UserList.Count; i++)
                    {
                        int row = i + 2;
                        int column = 1;

                        worksheet.Cells[row, column++].Value = UserList[i].Stt;
                        worksheet.Cells[row, column++].Value = UserList[i].Name;
                        worksheet.Cells[row, column++].Value = UserList[i].Address;
                    }

                    package.Save();

                    MessageBox.Show("Thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error save: " + ex.Message);
                }
        }
    }
}
