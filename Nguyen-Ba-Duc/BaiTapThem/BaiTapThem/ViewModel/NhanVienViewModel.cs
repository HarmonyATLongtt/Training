using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BaiTapThem.Model;
using BaiTapThem.View;
using Microsoft.Win32;
using OfficeOpenXml;

namespace BaiTapThem.ViewModel
{
    public class NhanVienViewModel : BaseViewModel
    {
        private string filePath = "";

        private bool _isBirthdaySelected = true;

        public bool IsBirthdaySelected
        {
            get { return _isBirthdaySelected; }
            set
            {
                _isBirthdaySelected = value;
                UpdateVisibility();
                OnPropertyChanged(nameof(IsBirthdaySelected));
                OnPropertyChanged(nameof(IsAgeSelected));
                OnPropertyChanged(nameof(SelectedHeader));
            }
        }

        public bool IsAgeSelected => !IsBirthdaySelected;
        public string SelectedHeader => IsBirthdaySelected ? "Ngay Sinh" : "Tuoi";

        private Visibility _datePickerIsVisible = Visibility.Visible;

        public Visibility DatePickerIsVisible
        {
            get { return _datePickerIsVisible; }
            set { _datePickerIsVisible = value; OnPropertyChanged(nameof(DatePickerIsVisible)); }
        }

        private Visibility _textBlockIsVisible = Visibility.Hidden;

        public Visibility TextBlockIsVisible
        {
            get { return _textBlockIsVisible; }
            set { _textBlockIsVisible = value; OnPropertyChanged(nameof(TextBlockIsVisible)); }
        }

        private readonly NhanVienView _nvView;
        public PhongBan selectedPhongBan { get; set; }

        private ObservableCollection<NhanVien> _dsNhanVien;

        public ObservableCollection<NhanVien> DsNhanVien
        {
            get { return _dsNhanVien; }
            set
            {
                _dsNhanVien = value;
                OnPropertyChanged();
            }
        }

        private NhanVien _editableItem;

        public NhanVien EditableItem
        {
            get { return _editableItem; }
            set
            {
                _editableItem = value;
                OnPropertyChanged();
            }
        }

        private NhanVien _selectedItem;

        public NhanVien SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    EditableItem = new NhanVien
                    {
                        Ten = _selectedItem.Ten,
                        NgaySinh = _selectedItem.NgaySinh,
                        SoCanCuoc = _selectedItem.SoCanCuoc,
                        NgayKyHopDong = _selectedItem.NgayKyHopDong,
                        ThamNien = _selectedItem.ThamNien,
                        ChucVu = _selectedItem.ChucVu,
                        Luong = _selectedItem.Luong,
                        DiemNhanVien = _selectedItem.DiemNhanVien
                    };
                }

                OnPropertyChanged();
            }
        }

        public RelayCommand suaNhanVienCommand { get; set; }
        public RelayCommand xoaNhanVienCommand { get; set; }
        public RelayCommand OkCommand { get; set; }
        public RelayCommand themNhanVienCommand { get; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand exportNhanVienCommand { get; set; }

        public NhanVienViewModel(PhongBan selectedPhongBan, NhanVienView nhanVienView)
        {
            _nvView = nhanVienView;

            if (selectedPhongBan.DsNhanVien?.Count > 0)
            {
                DsNhanVien = new ObservableCollection<NhanVien>(selectedPhongBan.DsNhanVien);
                SelectedItem = DsNhanVien.FirstOrDefault();
            }
            else
            {
                selectedPhongBan.DsNhanVien = new ObservableCollection<NhanVien>();
                DsNhanVien = new ObservableCollection<NhanVien>(selectedPhongBan.DsNhanVien);
                EditableItem = new NhanVien();
            }

            themNhanVienCommand = new RelayCommand(AddNhanVien);
            suaNhanVienCommand = new RelayCommand(UpdateNhanVien);
            xoaNhanVienCommand = new RelayCommand(DeleteNhanVien);
            OkCommand = new RelayCommand(OkClick);
            CancelCommand = new RelayCommand(CancelClick);
            exportNhanVienCommand = new RelayCommand(ExportFile);
        }

        private void AddNhanVien(object p)
        {
            if (!string.IsNullOrEmpty(EditableItem.Ten) && !string.IsNullOrEmpty(EditableItem.SoCanCuoc) && !string.IsNullOrEmpty(EditableItem.ChucVu)
                   && EditableItem.NgaySinh > DateTime.MinValue && EditableItem.NgayKyHopDong > DateTime.MinValue
                   && EditableItem.ThamNien > 0 && EditableItem.Luong > 0 && EditableItem.DiemNhanVien > 0)
            {
                bool kq = false;
                for (int i = 0; i < DsNhanVien.Count(); i++)
                {
                    if (DsNhanVien[i].Ten.Equals(EditableItem.Ten))
                    {
                        kq = true;
                        break;
                    }
                }
                if (kq == false)
                {
                    NhanVien newNhanVien = new NhanVien
                    {
                        Ten = EditableItem.Ten,
                        NgaySinh = EditableItem.NgaySinh,
                        SoCanCuoc = EditableItem.SoCanCuoc,
                        NgayKyHopDong = EditableItem.NgayKyHopDong,
                        ThamNien = EditableItem.ThamNien,
                        ChucVu = EditableItem.ChucVu,
                        Luong = EditableItem.Luong,
                        DiemNhanVien = EditableItem.DiemNhanVien
                    };
                    DsNhanVien.Add(newNhanVien);
                    EditableItem = new NhanVien();
                }
                else
                {
                    MessageBox.Show("Trung ten");
                }
            }
            else
            {
                MessageBox.Show("Xin vui long nhap du thong tin");
            }
        }

        private void UpdateNhanVien(object p)
        {
            if (SelectedItem != null)
            {
                for (int i = 0; i < DsNhanVien.Count; i++)
                {
                    if (DsNhanVien[i].Ten == EditableItem.Ten)
                    {
                        if (!string.IsNullOrEmpty(EditableItem.SoCanCuoc) && !string.IsNullOrEmpty(EditableItem.ChucVu)
                              && EditableItem.NgaySinh > DateTime.MinValue && EditableItem.NgayKyHopDong > DateTime.MinValue
                              && EditableItem.ThamNien > 0 && EditableItem.Luong > 0 && EditableItem.DiemNhanVien > 0)
                        {
                            DsNhanVien[i].NgaySinh = EditableItem.NgaySinh;
                            DsNhanVien[i].SoCanCuoc = EditableItem.SoCanCuoc;
                            DsNhanVien[i].NgayKyHopDong = EditableItem.NgayKyHopDong;
                            DsNhanVien[i].ThamNien = EditableItem.ThamNien;
                            DsNhanVien[i].ChucVu = EditableItem.ChucVu;
                            DsNhanVien[i].Luong = EditableItem.Luong;
                            DsNhanVien[i].DiemNhanVien = EditableItem.DiemNhanVien;
                        }
                        else
                        {
                            MessageBox.Show("Xin vui long nhap du thong tin");
                        }
                    }
                }
                var items = DsNhanVien;
                DsNhanVien = null;
                DsNhanVien = items;
                OnPropertyChanged(nameof(DsNhanVien));
            }
            else
            {
                MessageBox.Show("Xin chon nhan vien");
            }
        }

        private void DeleteNhanVien(object p)
        {
            if (SelectedItem != null)
            {
                DsNhanVien.Remove(SelectedItem);
                EditableItem = new NhanVien();
            }
            else
            {
                MessageBox.Show("Xin chon nhan vien");
            }
        }

        private void OkClick(object p)
        {
            _nvView.DialogResult = true;

            _nvView.Close();
        }

        private void CancelClick(object p)
        {
            _nvView.DialogResult = false;

            _nvView.Close();
        }

        private void UpdateVisibility()
        {
            DatePickerIsVisible = IsBirthdaySelected ? Visibility.Visible : Visibility.Hidden;
            TextBlockIsVisible = IsBirthdaySelected ? Visibility.Hidden : Visibility.Visible;
        }

        private void ExportFile(object p)
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

            //DataTable dataTable = new DataTable();

            //dataTable.Columns.Add("Ten Nhan Vien", typeof(string));
            //dataTable.Columns.Add("Ngay Sinh", typeof(DateTime));
            //dataTable.Columns.Add("So Can Cuoc", typeof(string));
            //dataTable.Columns.Add("Ngay Ky Hop Dong", typeof(DateTime));
            //dataTable.Columns.Add("Tham Nien", typeof(double));
            //dataTable.Columns.Add("Chuc Vu", typeof(string));
            //dataTable.Columns.Add("Luong", typeof(double));
            //dataTable.Columns.Add("Diem Nhan Vien", typeof(double));

            //foreach (var nhanVien in DsNhanVien)
            //{
            //    DataRow row = dataTable.NewRow();
            //    row["Ten Nhan Vien"] = nhanVien.Ten;
            //    row["Ngay Sinh"] = nhanVien.NgaySinh.ToString("dd/MM/yyyy");
            //    row["So Can Cuoc"] = nhanVien.SoCanCuoc;
            //    row["Ngay Ky Hop Dong"] = nhanVien.NgayKyHopDong.ToString("dd/MM/yyyy");
            //    row["Tham Nien"] = nhanVien.ThamNien;
            //    row["Chuc Vu"] = nhanVien.ChucVu;
            //    row["Luong"] = nhanVien.Luong;
            //    row["Diem Nhan Vien"] = nhanVien.DiemNhanVien;
            //    dataTable.Rows.Add(row);
            //}

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Nhan Vien");

                worksheet.Cells[1, 1].Value = "Ten Nhan Vien";
                worksheet.Cells[1, 2].Value = "Ngay Sinh";
                worksheet.Cells[1, 3].Value = "So Can Cuoc";
                worksheet.Cells[1, 4].Value = "Ngay Ky Hop Dong";
                worksheet.Cells[1, 5].Value = "Tham Nien";
                worksheet.Cells[1, 6].Value = "Chuc Vu";
                worksheet.Cells[1, 7].Value = "Luong";
                worksheet.Cells[1, 8].Value = "Diem Nhan Vien";

                for (int i = 0; i < DsNhanVien.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = DsNhanVien[i].Ten;
                    worksheet.Cells[i + 2, 2].Value = DsNhanVien[i].NgaySinh.ToString("yyyy/MM/dd");
                    worksheet.Cells[i + 2, 3].Value = DsNhanVien[i].SoCanCuoc.ToString();
                    worksheet.Cells[i + 2, 4].Value = DsNhanVien[i].NgayKyHopDong.ToString("yyyy/MM/dd");
                    worksheet.Cells[i + 2, 5].Value = DsNhanVien[i].ThamNien;
                    worksheet.Cells[i + 2, 6].Value = DsNhanVien[i].ChucVu;
                    worksheet.Cells[i + 2, 7].Value = DsNhanVien[i].Luong;
                    worksheet.Cells[i + 2, 8].Value = DsNhanVien[i].DiemNhanVien;
                }

                package.SaveAs(new FileInfo(filePath));
            }
            MessageBox.Show("Xuất excel thành công!");
        }
    }
}