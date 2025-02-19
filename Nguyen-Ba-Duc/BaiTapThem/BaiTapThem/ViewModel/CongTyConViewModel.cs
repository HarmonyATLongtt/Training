using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.Model;
using BaiTapThem.View;
using System.Windows.Input;
using System.Windows;

namespace BaiTapThem.ViewModel
{
    public class CongTyConViewModel : BaseViewModel
    {
        private readonly CongTyConView _ctyConView;

        private ObservableCollection<ChildCompany> _dsCongTyCon;

        public ObservableCollection<ChildCompany> DsCongTyCon
        {
            get { return _dsCongTyCon; }
            set
            {
                _dsCongTyCon = value;
                OnPropertyChanged();
            }
        }

        private ChildCompany _editableItem;

        public ChildCompany EditableItem
        {
            get { return _editableItem; }
            set
            {
                _editableItem = value;
                OnPropertyChanged();
            }
        }

        private ChildCompany _selectedItem;

        public ChildCompany SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    EditableItem = new ChildCompany
                    {
                        Ten = _selectedItem.Ten,
                        Diachi = _selectedItem.Diachi,
                        TongGiamDoc = _selectedItem.TongGiamDoc,
                        NgayThanhLap = _selectedItem.NgayThanhLap,
                        SoGiayPhepKinhDoanh = _selectedItem.SoGiayPhepKinhDoanh,
                        MieuTa = _selectedItem.MieuTa,
                        TongSoNhanVien = _selectedItem.TongSoNhanVien
                    };
                }
                OnPropertyChanged();
            }
        }

        public RelayCommand themCongTyCommand { get; }
        public RelayCommand suaCongTyCommand { get; }
        public RelayCommand xoaCongTyCommand { get; }
        public RelayCommand xemDsPhongBanCommand { get; }
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }

        public CongTyConViewModel(ParentCompany ctyMe, CongTyConView ctyConView)
        {
            _ctyConView = ctyConView;

            if (ctyMe.DsCongTyCon?.Count > 0)
            {
                DsCongTyCon = new ObservableCollection<ChildCompany>(ctyMe.DsCongTyCon);
                SelectedItem = DsCongTyCon.FirstOrDefault();
            }
            else
            {
                ctyMe.DsCongTyCon = new ObservableCollection<ChildCompany>();
                DsCongTyCon = new ObservableCollection<ChildCompany>(ctyMe.DsCongTyCon);
                EditableItem = new ChildCompany();
            }

            themCongTyCommand = new RelayCommand(AddCongTy);
            suaCongTyCommand = new RelayCommand(UpdateCongTy);
            xoaCongTyCommand = new RelayCommand(DeleteCongTy);
            xemDsPhongBanCommand = new RelayCommand(SeePhongBan);
            OkCommand = new RelayCommand(OkClick);
            CancelCommand = new RelayCommand(CancelClick);
        }

        private void AddCongTy(object p)
        {
            if (!string.IsNullOrEmpty(EditableItem.Ten) && !string.IsNullOrEmpty(EditableItem.Diachi) && !string.IsNullOrEmpty(EditableItem.TongGiamDoc) && !string.IsNullOrEmpty(EditableItem.SoGiayPhepKinhDoanh) && !string.IsNullOrEmpty(EditableItem.MieuTa))
            {
                bool kq = false;
                for (int i = 0; i < DsCongTyCon.Count(); i++)
                {
                    if (DsCongTyCon[i].Ten.Equals(EditableItem.Ten))
                    {
                        kq = true;
                        break;
                    }
                }
                if (kq == false)
                {
                    DsCongTyCon.Add(new ChildCompany
                    {
                        Ten = EditableItem.Ten,
                        Diachi = EditableItem.Diachi,
                        TongGiamDoc = EditableItem.TongGiamDoc,
                        SoGiayPhepKinhDoanh = EditableItem.SoGiayPhepKinhDoanh,
                        MieuTa = EditableItem.MieuTa,
                    });
                    EditableItem = new ChildCompany();
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

        private void UpdateCongTy(object p)
        {
            if (SelectedItem != null)
            {
                for (int i = 0; i < DsCongTyCon.Count; i++)
                {
                    if (DsCongTyCon[i].Ten == EditableItem.Ten)
                    {
                        if (!string.IsNullOrEmpty(EditableItem.Diachi) && !string.IsNullOrEmpty(EditableItem.TongGiamDoc) && !string.IsNullOrEmpty(EditableItem.SoGiayPhepKinhDoanh) && !string.IsNullOrEmpty(EditableItem.MieuTa))
                        {
                            DsCongTyCon[i].Diachi = EditableItem.Diachi;
                            DsCongTyCon[i].TongGiamDoc = EditableItem.TongGiamDoc;
                            DsCongTyCon[i].SoGiayPhepKinhDoanh = EditableItem.SoGiayPhepKinhDoanh;
                            DsCongTyCon[i].MieuTa = EditableItem.MieuTa;
                        }
                        else
                        {
                            MessageBox.Show("Xin vui long nhap du thong tin");
                        }
                    }
                }
                var items = DsCongTyCon;
                DsCongTyCon = null;
                DsCongTyCon = items;
                OnPropertyChanged(nameof(DsCongTyCon));
            }
            else
            {
                MessageBox.Show("Xin chon cong ty");
            }
        }

        private void DeleteCongTy(object p)
        {
            if (SelectedItem != null)
            {
                DsCongTyCon.Remove(SelectedItem);
                EditableItem = new ChildCompany();
            }
            else
            {
                MessageBox.Show("Xin chon cong ty");
            }
        }

        private void SeePhongBan(object p)
        {
            if (SelectedItem != null)
            {
                var pbView = new PhongBanView();
                var pbViewModel = new PhongBanViewModel(SelectedItem, pbView);
                pbView.DataContext = pbViewModel;
                _ctyConView.Hide();
                bool? result = pbView.ShowDialog();

                if (result == true)
                {
                    for (int i = 0; i < DsCongTyCon.Count; i++)
                    {
                        if (DsCongTyCon[i].Ten == SelectedItem.Ten)
                        {
                            DsCongTyCon[i].DsPhongBan = pbViewModel.DsPhongBan;
                            //DsCongTyCon[i].DsNhanVienDiemCao = pbViewModel.DsNhanVienDiemCao;
                            int tongNV = 0;
                            for (int j = 0; j < pbViewModel.DsPhongBan.Count(); j++)
                            {
                                tongNV = tongNV + pbViewModel.DsPhongBan[j].SoNhanVien;
                            }
                            DsCongTyCon[i].TongSoNhanVien = tongNV;
                            //SelectedItem = DsCongTyCon[i];
                            EditableItem = new ChildCompany();
                        }
                    }

                    var items = DsCongTyCon;
                    DsCongTyCon = null;
                    DsCongTyCon = items;
                    OnPropertyChanged(nameof(DsCongTyCon));
                    _ctyConView.ShowDialog();
                }
                if (result == false)
                {
                    for (int i = 0; i < DsCongTyCon.Count; i++)
                    {
                        if (DsCongTyCon[i].Ten == SelectedItem.Ten)
                        {
                            DsCongTyCon[i].DsPhongBan = pbViewModel.DsPhongBan;
                            int tongNV = 0;
                            for (int j = 0; j < pbViewModel.DsPhongBan.Count(); j++)
                            {
                                tongNV = tongNV + pbViewModel.DsPhongBan[j].SoNhanVien;
                            }
                            DsCongTyCon[i].TongSoNhanVien = tongNV;
                            EditableItem = new ChildCompany();
                        }
                    }

                    var items = DsCongTyCon;
                    DsCongTyCon = null;
                    DsCongTyCon = items;
                    OnPropertyChanged(nameof(DsCongTyCon));
                    _ctyConView.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Xin chon cong ty");
            }
        }

        private void OkClick(object p)
        {
            _ctyConView.DialogResult = true;

            _ctyConView.Close();
        }

        private void CancelClick(object p)
        {
            _ctyConView.DialogResult = false;

            _ctyConView.Close();
        }
    }
}