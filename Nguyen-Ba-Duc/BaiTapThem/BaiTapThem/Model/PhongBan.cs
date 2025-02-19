using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BaiTapThem.ViewModel;

namespace BaiTapThem.Model
{
    public class PhongBan : BaseViewModel
    {
        private string _ten;

        public string Ten
        {
            get { return _ten; }
            set
            {
                _ten = value;
                OnPropertyChanged(nameof(Ten));
            }
        }

        private string _truongPhong;

        public string TruongPhong
        {
            get { return _truongPhong; }
            set
            {
                _truongPhong = value;
                OnPropertyChanged(nameof(TruongPhong));
            }
        }

        private string _phoPhong;

        public string PhoPhong
        {
            get { return _phoPhong; }
            set
            {
                _phoPhong = value;
                OnPropertyChanged(nameof(PhoPhong));
            }
        }

        public int SoNhanVien { get; set; }
        public ObservableCollection<NhanVien> DsNhanVien { get; set; }

        public PhongBan()
        {
            DsNhanVien = new ObservableCollection<NhanVien>();
            SoNhanVien = 0;
        }
    }
}