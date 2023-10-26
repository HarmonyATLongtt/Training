using APP_WPF_MVVM.Model;
using APP_WPF_MVVM.ViewModel;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace APP_WPF_MVVM.View
{
    /// <summary>
    /// Interaction logic for view.xaml
    /// </summary>
    public partial class view : UserControl
    {
        public view()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

       /* private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.ShowDialog();
                //  ofd.FileName = @"Documenten";

                // tạo ra danh sách UserInfo rỗng để hứng dữ liệu.
                List<Students> userList = new List<Students>();

                // mở file excel
                var package = new ExcelPackage(new FileInfo(ofd.FileName));

                // lấy ra sheet đầu tiên để thao tác
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

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
                        // tạo UserInfo từ dữ liệu đã lấy được
                        Students user = new Students()
                        {
                            Id = id,
                            Name = name,
                            Age = age,
                            Address = address,
                            Class = class1,
                            School = school
                        };

                        // add UserInfo vào danh sách userList
                        userList.Add(user);

                    }
                    catch (Exception exe)
                    {

                    }
                }
                dtg1.ItemsSource = userList;
            }
            catch ( Exception ee)
            {
                MessageBox.Show("Co loi xay ra","Thong bao", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
        }*/
    }
}
