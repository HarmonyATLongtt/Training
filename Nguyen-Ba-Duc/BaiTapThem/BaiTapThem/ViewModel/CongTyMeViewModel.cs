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
    public class CongTyMeViewModel : BaseViewModel
    {
        private KhoiTaoDuLieu DuLieu { get; set; }

        private readonly CongTyMeView _ctyMeView;

        private ParentCompany _congTyMe;

        public ParentCompany CongTyMe
        {
            get { return _congTyMe; }
            set
            {
                _congTyMe = value;
                //OnPropertyChanged();
            }
        }

        private ParentCompany _editableItem;

        public ParentCompany EditableItem
        {
            get { return _editableItem; }
            set
            {
                _editableItem = value;
                //OnPropertyChanged();
            }
        }

        public RelayCommand suaCongTyMeCommand { get; }

        public RelayCommand xemDsCongTyConCommand { get; }

        public CongTyMeViewModel(CongTyMeView ctyMeView)
        {
            _ctyMeView = ctyMeView;
            DuLieu = new KhoiTaoDuLieu();
            CongTyMe = DuLieu.CongTyMe;
            EditableItem = CongTyMe;

            //suaCongTyMeCommand = new RelayCommand(UpdateCongTy, NoUpdateCongTy);

            suaCongTyMeCommand = new RelayCommand(UpdateCongTy);
            xemDsCongTyConCommand = new RelayCommand(SeeCongTy);
        }

        //private bool NoUpdateCongTy(object p)
        //{
        //    return !string.IsNullOrEmpty(EditableItem.DiaChi) && !string.IsNullOrEmpty(EditableItem.TongGiamDoc) && !string.IsNullOrEmpty(EditableItem.MieuTa);

        //    //MessageBox.Show("Nhap du thong tin ban nhe");
        //}

        private void UpdateCongTy(object p)
        {
            if (!string.IsNullOrEmpty(EditableItem.DiaChi) && !string.IsNullOrEmpty(EditableItem.TongGiamDoc) && !string.IsNullOrEmpty(EditableItem.MieuTa))
            {
                CongTyMe.DiaChi = EditableItem.DiaChi;
                CongTyMe.TongGiamDoc = EditableItem.TongGiamDoc;
                CongTyMe.MieuTa = EditableItem.MieuTa;

                var item = CongTyMe;
                CongTyMe = null;
                CongTyMe = item;
                OnPropertyChanged(nameof(CongTyMe));
            }
            else
            {
                MessageBox.Show("Xin vui long nhap du thong tin");
            }
        }

        private void SeeCongTy(object p)
        {
            var ctyConView = new CongTyConView();
            var ctyConViewModel = new CongTyConViewModel(CongTyMe, ctyConView);
            ctyConView.DataContext = ctyConViewModel;
            _ctyMeView.Hide();
            bool? result = ctyConView.ShowDialog();

            if (result == true)
            {
                CongTyMe.DsCongTyCon = ctyConViewModel.DsCongTyCon;
                CongTyMe.TongCongTyCon = ctyConViewModel.DsCongTyCon.Count();
                int tongNV = 0;
                for (int i = 0; i < ctyConViewModel.DsCongTyCon.Count(); i++)
                {
                    tongNV = tongNV + ctyConViewModel.DsCongTyCon[i].TongSoNhanVien;
                }
                CongTyMe.TongNhanVien = tongNV;

                var item = CongTyMe;
                CongTyMe = null;
                CongTyMe = item;
                EditableItem = CongTyMe;
                OnPropertyChanged(nameof(CongTyMe));
                _ctyMeView.ShowDialog();
            }
            if (result == false)
            {
                int tongNV = 0;
                for (int i = 0; i < ctyConViewModel.DsCongTyCon.Count(); i++)
                {
                    tongNV = tongNV + ctyConViewModel.DsCongTyCon[i].TongSoNhanVien;
                }
                CongTyMe.TongNhanVien = tongNV;

                var item = CongTyMe;
                CongTyMe = null;
                CongTyMe = item;
                EditableItem = CongTyMe;
                OnPropertyChanged(nameof(CongTyMe));

                _ctyMeView.ShowDialog();
            }
        }
    }
}