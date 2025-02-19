using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.Model;

namespace BaiTapThem.ViewModel
{
    public class KhoiTaoDuLieu : BaseViewModel
    {
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

        private ParentCompany _congTyMe;

        public ParentCompany CongTyMe
        {
            get { return _congTyMe; }
            set
            {
                _congTyMe = value;
                OnPropertyChanged();
            }
        }

        public KhoiTaoDuLieu()
        {
            List<NhanVien> listNhanVien = new List<NhanVien>();
            NhanVien nv1 = new NhanVien { Ten = "Ba Duc", NgaySinh = new DateTime(1994, 2, 5), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 3, ChucVu = "Dev", Luong = 35000000, DiemNhanVien = 10 };
            NhanVien nv2 = new NhanVien { Ten = "Tran Anh", NgaySinh = new DateTime(1993, 2, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 2, ChucVu = "Dev", Luong = 12000000, DiemNhanVien = 8 };
            NhanVien nv3 = new NhanVien { Ten = "Duc Quy", NgaySinh = new DateTime(1994, 9, 9), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 23000000, DiemNhanVien = 9 };
            NhanVien nv4 = new NhanVien { Ten = "Hoang Tung", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            NhanVien nv5 = new NhanVien { Ten = "Dinh Luong", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            NhanVien nv6 = new NhanVien { Ten = "Thi Hong", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            NhanVien nv7 = new NhanVien { Ten = "Mai Lan", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            NhanVien nv8 = new NhanVien { Ten = "Linh Ngoc", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            NhanVien nv9 = new NhanVien { Ten = "Thi Van", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            NhanVien nv10 = new NhanVien { Ten = "Anh Tuyet", NgaySinh = new DateTime(1994, 8, 7), SoCanCuoc = "232232323", NgayKyHopDong = new DateTime(2025, 2, 15), ThamNien = 1, ChucVu = "Dev", Luong = 24000000, DiemNhanVien = 8 };
            listNhanVien.Add(nv1);
            listNhanVien.Add(nv2);
            listNhanVien.Add(nv3);
            listNhanVien.Add(nv4);
            listNhanVien.Add(nv5);
            listNhanVien.Add(nv6);
            listNhanVien.Add(nv7);
            listNhanVien.Add(nv8);
            listNhanVien.Add(nv9);
            listNhanVien.Add(nv10);
            DsNhanVien = new ObservableCollection<NhanVien>(listNhanVien);

            List<PhongBan> listPhongBan = new List<PhongBan>();

            PhongBan phongBan1 = new PhongBan { Ten = "Phong ky thuat", TruongPhong = "Nguyen Ba Duc", PhoPhong = "Nguyen Van A" };
            PhongBan phongBan2 = new PhongBan { Ten = "Phong tuyen dung", TruongPhong = "Nguyen Thi Hanh", PhoPhong = "Nguyen Thi Minh Ngoc" };
            PhongBan phongBan3 = new PhongBan { Ten = "Phong ke toan", TruongPhong = "Nguyen Thi Lan", PhoPhong = "Nguyen Thi Hong" };

            phongBan1.DsNhanVien.Add(nv1);
            phongBan1.DsNhanVien.Add(nv2);
            phongBan1.DsNhanVien.Add(nv3);
            phongBan1.DsNhanVien.Add(nv4);
            phongBan2.DsNhanVien.Add(nv5);
            phongBan2.DsNhanVien.Add(nv6);
            phongBan3.DsNhanVien.Add(nv7);
            phongBan3.DsNhanVien.Add(nv8);
            phongBan3.DsNhanVien.Add(nv9);
            phongBan3.DsNhanVien.Add(nv10);

            listPhongBan.Add(phongBan1);
            listPhongBan.Add(phongBan2);
            listPhongBan.Add(phongBan3);

            DsPhongBan = new ObservableCollection<PhongBan>(listPhongBan);

            List<ChildCompany> listCongTyCon = new List<ChildCompany>();

            ChildCompany congTyCon1 = new ChildCompany { Ten = "Harmony", Diachi = "Duy Tan", TongGiamDoc = "Nguyen Van A", NgayThanhLap = new DateTime(2000, 1, 1), SoGiayPhepKinhDoanh = "2323232", MieuTa = "Good" };
            ChildCompany congTyCon2 = new ChildCompany { Ten = "Harmony AT", Diachi = "Thai Ha", TongGiamDoc = "Nguyen Van B", NgayThanhLap = new DateTime(2010, 1, 1), SoGiayPhepKinhDoanh = "232113232", MieuTa = "Great" };

            congTyCon1.DsPhongBan.Add(phongBan1);
            congTyCon1.DsPhongBan.Add(phongBan2);
            congTyCon2.DsPhongBan.Add(phongBan3);

            listCongTyCon.Add(congTyCon1);
            listCongTyCon.Add(congTyCon2);

            DsCongTyCon = new ObservableCollection<ChildCompany>(listCongTyCon);

            CongTyMe = new ParentCompany
            {
                Ten = "Cong Ty Hai Hoa",
                DiaChi = "17 Duy Tan",
                TongGiamDoc = "Nguyen Van A",
                NgayThanhLap = new DateTime(2000, 01, 05),
                SoGiayPhepKinhDoanh = "123232323",
                MieuTa = "Excellent"
            };

            CongTyMe.DsCongTyCon = DsCongTyCon;

            CongTyMe.TongCongTyCon = DsCongTyCon.Count();
            CongTyMe.TongNhanVien = DsNhanVien.Count();

            foreach (ChildCompany ctyCon in DsCongTyCon)
            {
                List<NhanVien> listNhanVienDiemTren8 = new List<NhanVien>();
                int tongNhanVien = 0;

                foreach (PhongBan phongBan in ctyCon.DsPhongBan)
                {
                    phongBan.SoNhanVien = phongBan.DsNhanVien.Count();

                    tongNhanVien = tongNhanVien + phongBan.SoNhanVien;

                    foreach (NhanVien nhanVien in phongBan.DsNhanVien)
                    {
                        if (nhanVien.DiemNhanVien > 8)
                        {
                            ctyCon.DsNhanVienDiemCao.Add(nhanVien);
                        }
                    }
                }

                ctyCon.TongSoNhanVien = tongNhanVien;
            }
        }
    }
}