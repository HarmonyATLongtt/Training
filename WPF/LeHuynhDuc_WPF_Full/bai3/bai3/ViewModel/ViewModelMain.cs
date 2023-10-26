using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

//fixed
namespace bai3.ViewModel
{
    public class ViewModelMain : ViewModelBase
    {
        //Tao list luu tru du lieu cua excel
        private ObservableCollection<DataTable> _dataTable;
        public ObservableCollection<DataTable> ListDataTable
        {
            get { return _dataTable; }
            set { _dataTable = value; NotifyPropertyChanged(); }
        }
        //Tao doi tuong DataTable de binding DataGrid ben phan view
        private DataTable _data1;
        public DataTable DataDisplay
        {
            get { return _data1; }
            set
            {
                _data1 = value;
                NotifyPropertyChanged();
            }
        }
        //SheetNames - Lưu tên các sheets có trong file excel
        private ObservableCollection<string> _sheetNames;
        public ObservableCollection<string> SheetNames
        {
            get { return _sheetNames; }
            set { _sheetNames = value; NotifyPropertyChanged(); }
        }
        //Tao bien index de binding voi combobox
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    if (ListDataTable.Count > 1)
                    {
                        DataDisplay = ListDataTable[Math.Abs(value)];
                    }    
                        
                    _index = value; NotifyPropertyChanged();
                }

            }
        }
        //Tao bien hien thi khoang su dung trong sheet
        private string _range;
        public string Range
        {
            get { return _range; }
            set
            {
                _range = value;
                NotifyPropertyChanged();
            }
        }
        //Tao doi tuong kieu ICommand
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand CloseCommand { get; }

        public ViewModelMain()
        {
            ListDataTable = new ObservableCollection<DataTable>();
            Index = 0;//gia tri ban dau  
            DataDisplay = new DataTable();
            ImportCommand = new RelayCommand1(ImportFull);
            ExportCommand = new RelayCommand1(Export);
            CloseCommand = new RelayCommand1(CloseWin);
        }
        
        private void CloseWin()
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Thông báo", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
                Application.Current.MainWindow.Close();
        }
        //mo file va doc du lieu vao ListDataTable
        private void Import()
        {
            try
            {
                //Chon duong dan file
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.ShowDialog();
                if (string.IsNullOrEmpty(ofd.FileName))
                {
                    return;
                }
                // ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // mở file excel
                var package = new ExcelPackage(new FileInfo(ofd.FileName));
                //Lay ra ten cua cac sheet cho vao list SheetNames
                GetSheetNames(package);
                // lấy ra số lượng các sheet       
                int NunberSheet = package.Workbook.Worksheets.Count;
                ListDataTable = new ObservableCollection<DataTable>();

                //duyet tung sheet trong excel
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    //tạo đối tượng kiểu datatable dùng lưu trữ các sheet để add vào ListDataTable
                    DataTable data1 = new DataTable();

                    //Lấy header của sheet cho vào datatable
                    //MessageBox.Show(sheet.Dimension.ToString());
                    if (sheet.Dimension != null)
                    {
                        for (int i = sheet.Dimension.Start.Column; i <= sheet.Dimension.End.Column; i++)
                        {
                            //them cot cho DataTable
                            if (sheet.Cells[1, i].Value != null)
                                data1.Columns.Add(sheet.Cells[1, i].Value.ToString(), typeof(string));
                            else
                                data1.Columns.Add("Null");
                        }

                        //duyet tung dong cua sheet
                        for (int i = sheet.Dimension.Start.Row + 1; i <= sheet.Dimension.End.Row; i++)
                        {
                            //tao rowData de luu du lieu cua 1 dong
                            List<string> rowData = new List<string>();
                            // duyet tung cot  cua sheet
                            for (int j = 1; j <= sheet.Dimension.End.Column; j++)
                            {
                                //Kiem tra xem dong co du lieu khong
                                if (sheet.Cells[i, j].Value != null)
                                    //Neu co thi them du lieu vao
                                    rowData.Add(sheet.Cells[i, j].Value.ToString());
                                else
                                    //Neu khong co thi them vao rong
                                    rowData.Add("");
                                //MessageBox.Show(j.ToString());
                            }
                            //Them du lieu tren vao dong
                            data1.Rows.Add(rowData.ToArray());
                        }
                    }
                    ListDataTable.Add(data1);
                }
                MessageBox.Show("Đọc dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi xảy ra!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ImportFull()
        {
            Import();//Mo file va doc du lieu vao ListDataTable
            Index = 0;
            if (ListDataTable.Count > 0)
                 DataDisplay = ListDataTable[0];//gia tri ban dau
        }
        //Lay ra cac ten cua sheet co trong excel cho vao list SheetNames
        private void GetSheetNames(ExcelPackage package)
        {
            SheetNames = new ObservableCollection<string>();
            // Lấy ra tên của các sheet có trong excel
            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                SheetNames.Add(worksheet.Name);
            }
        }
        private void Export()
        {
            try
            {
                string filePath = "";
                // tạo SaveFileDialog để lưu file excel
                SaveFileDialog dialog = new SaveFileDialog();

                // chỉ lọc ra các file có định dạng Excel
                dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";
                if (dialog.ShowDialog() == true)
                {
                    filePath = dialog.FileName;
                }
                // nếu đường dẫn null hoặc rỗng thì báo không hợp lệ và return hàm
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                //ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                if (ListDataTable == null)//Kiểm tra xem đã có file để xuất chưa
                    MessageBox.Show("Chưa có file để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    using (ExcelPackage p = new ExcelPackage())
                    {
                        // đặt tên người tạo file
                        p.Workbook.Properties.Author = "DucLe";

                        // đặt tiêu đề cho file
                        p.Workbook.Properties.Title = "Excel_MVVM_ThucTap";

                        //Tạo một sheet để làm việc trên đó
                        int in1 = 0;
                        //duyet tung object trong ListDataTable
                        foreach (var data1 in ListDataTable) 
                        {
                            //Tao sheet tuong ung
                            p.Workbook.Worksheets.Add(SheetNames[in1]);
                            //Lay sheet ra de lam viec
                            ExcelWorksheet ws = p.Workbook.Worksheets[in1];
                            // đặt tên cho sheet
                            ws.Name = SheetNames[in1];
                            // fontsize mặc định cho cả sheet
                            ws.Cells.Style.Font.Size = 11;
                            // font family mặc định cho cả sheet
                            ws.Cells.Style.Font.Name = "Calibri";

                            // Tạo danh sách các column header
                            List<string> arrColumnHeader = new List<string>();
                            foreach (var col in ListDataTable[in1].Columns)
                            {
                                arrColumnHeader.Add(col.ToString());
                            }
                            // lấy ra số lượng cột cần dùng dựa vào số lượng header
                            var countColHeader = arrColumnHeader.Count();
                            int colIndex = 1;
                            int rowIndex = 1;
                            //tạo các header từ column header đã tạo từ bên trên
                            foreach (var item in arrColumnHeader)
                            {
                                var cell = ws.Cells[rowIndex, colIndex];
                                //gán giá trị
                                cell.Value = item;
                                colIndex++;
                            }

                            //duyet tung dong cua cua DataTable
                            for (int i = 0; i <= ListDataTable[in1].Rows.Count - 1; i++) 
                            {
                                //tao rowData de luu du lieu cua 1 dong
                                List<string> rowData = new List<string>();
                                // duyet tung cot  cua DataTable
                                for (int j = 0; j <= ListDataTable[in1].Columns.Count - 1; j++) 
                                {
                                    //gán giá trị cho từng ô trong sheet (vì dòng 1 đã gán header ở trên và excel bắt đầu từ 1 nên i+2, j+1
                                    ws.Cells[i + 2, j + 1].Value = ListDataTable[in1].Rows[i][ListDataTable[in1].Columns[j]].ToString();

                                }
                            }
                            in1++;
                        }
                        //Lưu file lại
                        Byte[] bin = p.GetAsByteArray();
                        File.WriteAllBytes(filePath, bin);
                        MessageBox.Show("Xuất excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
            }
            catch (Exception e)
            {
                MessageBox.Show("Có lỗi xảy ra!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
