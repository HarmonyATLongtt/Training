using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTapThem.Model
{
    public class ChildCompany
    {
        public string Ten { get; set; }
        public string Diachi { get; set; }
        public string TongGiamDoc { get; set; }
        public DateTime NgayThanhLap { get; set; }
        public string SoGiayPhepKinhDoanh { get; set; }
        public string MieuTa { get; set; }
        public int TongSoNhanVien { get; set; }
        public ObservableCollection<NhanVien> DsNhanVienDiemCao { get; set; }
        public ObservableCollection<PhongBan> DsPhongBan { get; set; }

        public ChildCompany()
        {
            DsPhongBan = new ObservableCollection<PhongBan>();
            TongSoNhanVien = 0;
            NgayThanhLap = DateTime.Now;
            DsNhanVienDiemCao = new ObservableCollection<NhanVien>();
        }
    }
}