using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTapThem.Model
{
    public class ParentCompany
    {
        public string Ten { get; set; }
        public string DiaChi { get; set; }
        public string TongGiamDoc { get; set; }
        public DateTime NgayThanhLap { get; set; }
        public string SoGiayPhepKinhDoanh { get; set; }
        public string MieuTa { get; set; }
        public int TongNhanVien { get; set; }
        public int TongCongTyCon { get; set; }
        public ObservableCollection<ChildCompany> DsCongTyCon { get; set; }

        public ParentCompany()
        {
            DsCongTyCon = new ObservableCollection<ChildCompany>();
        }
    }
}