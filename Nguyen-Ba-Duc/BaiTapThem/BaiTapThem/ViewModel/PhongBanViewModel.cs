using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.Model;
using System.Windows.Input;
using System.Windows;
using BaiTapThem.View;
using System.Runtime.CompilerServices;

namespace BaiTapThem.ViewModel
{
    public class PhongBanViewModel : BaseViewModel
    {
        private readonly PhongBanView _pbView;

        private ObservableCollection<PhongBan> _dsPhongBan;

        public ObservableCollection<PhongBan> DsPhongBan
        {
            get { return _dsPhongBan; }
            set
            {
                _dsPhongBan = value;
                OnPropertyChanged();
            }
        }

        private PhongBan _editableItem;

        public PhongBan EditableItem
        {
            get { return _editableItem; }
            set
            {
                _editableItem = value;
                OnPropertyChanged();
            }
        }

        private PhongBan _selectedItem;

        public PhongBan SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    EditableItem = new PhongBan
                    {
                        Ten = _selectedItem.Ten,
                        TruongPhong = _selectedItem.TruongPhong,
                        PhoPhong = _selectedItem.PhoPhong,
                        SoNhanVien = _selectedItem.SoNhanVien,
                    };
                }
                OnPropertyChanged();
            }
        }

        public RelayCommand themPhongBanCommand { get; }
        public RelayCommand suaPhongBanCommand { get; }
        public RelayCommand xoaPhongBanCommand { get; }
        public RelayCommand xemdsNhanVienCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        public PhongBanViewModel(ChildCompany selectedCongTy, PhongBanView pbView)
        {
            _pbView = pbView;

            if (selectedCongTy.DsPhongBan?.Count > 0)
            {
                DsPhongBan = new ObservableCollection<PhongBan>(selectedCongTy.DsPhongBan);
                SelectedItem = DsPhongBan.FirstOrDefault();
            }
            else
            {
                selectedCongTy.DsPhongBan = new ObservableCollection<PhongBan>();
                DsPhongBan = new ObservableCollection<PhongBan>(selectedCongTy.DsPhongBan);
                EditableItem = new PhongBan();
            }

            themPhongBanCommand = new RelayCommand(AddPhongBan);
            suaPhongBanCommand = new RelayCommand(UpdatePhongBan);
            xoaPhongBanCommand = new RelayCommand(DeletePhongBan);
            xemdsNhanVienCommand = new RelayCommand(SeeNhanVien);
            OkCommand = new RelayCommand(OkClick);
            CancelCommand = new RelayCommand(CancelClick);
        }

        private void AddPhongBan(object p)
        {
            if (!string.IsNullOrEmpty(EditableItem.Ten) && !string.IsNullOrEmpty(EditableItem.TruongPhong) && !string.IsNullOrEmpty(EditableItem.PhoPhong))
            {
                bool kq = false;
                for (int i = 0; i < DsPhongBan.Count(); i++)
                {
                    if (DsPhongBan[i].Ten.Equals(EditableItem.Ten))
                    {
                        kq = true;
                        break;
                    }
                }
                if (kq == false)
                {
                    DsPhongBan.Add(new PhongBan
                    {
                        Ten = EditableItem.Ten,
                        TruongPhong = EditableItem.TruongPhong,
                        PhoPhong = EditableItem.PhoPhong
                    });
                    EditableItem = new PhongBan();
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

        private void UpdatePhongBan(object p)
        {
            if (SelectedItem != null)
            {
                for (int i = 0; i < DsPhongBan.Count; i++)
                {
                    if (DsPhongBan[i].Ten == EditableItem.Ten)
                    {
                        if (!string.IsNullOrEmpty(EditableItem.TruongPhong) && !string.IsNullOrEmpty(EditableItem.PhoPhong))
                        {
                            DsPhongBan[i].TruongPhong = EditableItem.TruongPhong;
                            DsPhongBan[i].PhoPhong = EditableItem.PhoPhong;
                        }
                        else
                        {
                            MessageBox.Show("Xin vui long nhap du thong tin");
                        }
                    }
                }
                var items = DsPhongBan;
                DsPhongBan = null;
                DsPhongBan = items;
                OnPropertyChanged(nameof(DsPhongBan));
            }
            else
            {
                MessageBox.Show("Xin chon phong ban");
            }
        }

        private void DeletePhongBan(object p)
        {
            if (SelectedItem != null)
            {
                DsPhongBan.Remove(SelectedItem);
                EditableItem = new PhongBan();
            }
            else
            {
                MessageBox.Show("Xin chon phong ban");
            }
        }

        private void SeeNhanVien(object p)
        {
            if (SelectedItem != null)
            {
                var nvView = new NhanVienView();
                var nvViewModel = new NhanVienViewModel(SelectedItem, nvView);
                nvView.DataContext = nvViewModel;
                _pbView.Hide();
                bool? result = nvView.ShowDialog();

                if (result == true)
                {
                    for (int i = 0; i < DsPhongBan.Count; i++)
                    {
                        if (DsPhongBan[i].Ten == SelectedItem.Ten)
                        {
                            DsPhongBan[i].DsNhanVien = nvViewModel.DsNhanVien;
                            DsPhongBan[i].SoNhanVien = nvViewModel.DsNhanVien.Count();

                            EditableItem = new PhongBan();
                        }
                    }

                    var items = DsPhongBan;
                    DsPhongBan = null;
                    DsPhongBan = items;
                    OnPropertyChanged(nameof(DsPhongBan));
                    _pbView.ShowDialog();
                }
                if (result == false)
                {
                    EditableItem = new PhongBan();
                    _pbView.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Xin chon phong ban");
            }
        }

        private void OkClick(object p)
        {
            _pbView.DialogResult = true;

            _pbView.Close();
        }

        private void CancelClick(object p)
        {
            _pbView.DialogResult = false;

            _pbView.Close();
        }
    }
}