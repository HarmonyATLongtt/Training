using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toDuc26102023.Models
{
    public class Tamgiac
    {
        public XYZ Dinh1 { get; set; }
        public XYZ Dinh2 { get; set; }
        public XYZ Dinh3 { get; set; }
        public Tamgiac(XYZ dinh1, XYZ dinh2, XYZ dinh3)
        {
            double d12 = Math.Sqrt((dinh1.X - dinh2.X) * (dinh1.X - dinh2.X) + (dinh1.Y - dinh2.Y) * (dinh1.Y - dinh2.Y));
            double d13 = Math.Sqrt((dinh1.X - dinh3.X) * (dinh1.X - dinh3.X) + (dinh1.Y - dinh3.Y) * (dinh1.Y - dinh3.Y));
            double d23 = Math.Sqrt((dinh3.X - dinh2.X) * (dinh3.X - dinh2.X) + (dinh3.Y - dinh2.Y) * (dinh3.Y - dinh2.Y));
            if (d12 > d13 && d12 > d23)
            {
                Dinh1 = dinh3;
                Dinh2 = dinh1;
                Dinh3 = dinh2;
            }
            else if (d13 > d12 && d13 > d23)
            {
                Dinh1 = dinh2;
                Dinh2 = dinh1;
                Dinh3 = dinh3;
            }
            else
            {
                Dinh1 = dinh1;
                Dinh2 = dinh2;
                Dinh3 = dinh3;
            }
        }
        //Chân đường cao
        public XYZ ChanDuongCao()
        {
            Line li = Line.CreateBound(Dinh2, Dinh3);
            return li.Project(Dinh1).XYZPoint;
        }
        //Độ dài từ hình chiếu đỉnh xuống đáy và đỉnh của đáy
        public double CanhHuyen()
        {
            XYZ trai;
            if (Dinh2.X < Dinh3.X)
                trai = Dinh2;
            else if (Dinh2.X == Dinh3.X && Dinh2.Y < Dinh3.Y)
                trai = Dinh2;
            else trai = Dinh3;
            return trai.DistanceTo(Dinh1);
        }
        //Do cao tam giac
        public double DoCao()
        {
            return Math.Sqrt((ChanDuongCao().X - Dinh1.X) * (ChanDuongCao().X - Dinh1.X) + (ChanDuongCao().Y - Dinh1.Y) * (ChanDuongCao().Y - Dinh1.Y));
        }
        //Do dai day
        public double DoDaiDay()
        {
            return Math.Sqrt((Dinh3.X - Dinh2.X) * (Dinh3.X - Dinh2.X) + (Dinh3.Y - Dinh2.Y) * (Dinh3.Y - Dinh2.Y));
        }
        //Trung điểm cạnh dài nhất
        public XYZ TrungDiemCanhDaiNhat()
        {
            return (Dinh2 + Dinh3) / 2.0;
        }
        //Lấy ra trọng tâm tam giác
        public XYZ TrongTam()
        {
            return (Dinh1 + Dinh2 + Dinh3) / 3.0;
        }
        public double sign(XYZ p1, XYZ p2, XYZ p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }
        //Kiểm tra xem điểm có nằm trong tam giác không
        public bool PointInTriangle(XYZ point)
        {
            bool b1, b2, b3;

            b1 = sign(point, Dinh1, Dinh2) <= -0.0001f;
            b2 = sign(point, Dinh2, Dinh3) <= -0.0001f;
            b3 = sign(point, Dinh3, Dinh1) <= -0.0001f;

            return ((b1 == b2) && (b2 == b3));
        }
        public double DienTich()
        {
            double c1 = Math.Sqrt((Dinh1.X - Dinh2.X) * (Dinh1.X - Dinh2.X) + (Dinh1.Y - Dinh2.Y) * (Dinh1.Y - Dinh2.Y));
            double c2 = Math.Sqrt((Dinh1.X - Dinh3.X) * (Dinh1.X - Dinh3.X) + (Dinh1.Y - Dinh3.Y) * (Dinh1.Y - Dinh3.Y));
            double c3 = Math.Sqrt((Dinh3.X - Dinh2.X) * (Dinh3.X - Dinh2.X) + (Dinh3.Y - Dinh2.Y) * (Dinh3.Y - Dinh2.Y));
            double ncv = 0.5 * (c1 + c2 + c3);
            return Math.Sqrt(ncv * (ncv - c1) * (ncv - c2) * (ncv - c3));
        }
        public bool CanhChung_CatQua(List<Tamgiac> tams)
        {
            foreach (Tamgiac tam in tams)
            {
                if (equalPoints(Dinh1, tam.Dinh1) && equalPoints(Dinh2, tam.Dinh2)) return false;
                if (equalPoints(Dinh1, tam.Dinh1) && equalPoints(Dinh2, tam.Dinh3)) return false;
                if (equalPoints(Dinh1, tam.Dinh1) && equalPoints(Dinh3, tam.Dinh2)) return false;
                if (equalPoints(Dinh1, tam.Dinh1) && equalPoints(Dinh3, tam.Dinh3)) return false;
                if (equalPoints(Dinh1, tam.Dinh2) && equalPoints(Dinh2, tam.Dinh1)) return false;
                if (equalPoints(Dinh1, tam.Dinh2) && equalPoints(Dinh2, tam.Dinh3)) return false;
                if (equalPoints(Dinh1, tam.Dinh2) && equalPoints(Dinh3, tam.Dinh1)) return false;
                if (equalPoints(Dinh1, tam.Dinh2) && equalPoints(Dinh3, tam.Dinh3)) return false;
                if (equalPoints(Dinh1, tam.Dinh3) && equalPoints(Dinh2, tam.Dinh1)) return false;
                if (equalPoints(Dinh1, tam.Dinh3) && equalPoints(Dinh2, tam.Dinh2)) return false;
                if (equalPoints(Dinh1, tam.Dinh3) && equalPoints(Dinh3, tam.Dinh1)) return false;
                if (equalPoints(Dinh1, tam.Dinh3) && equalPoints(Dinh3, tam.Dinh2)) return false;
            }

            return true;
        }
        public bool equalPoints(XYZ a, XYZ b)
        {
            if ((Math.Round(a.X, 5) == Math.Round(b.X, 5)) && (Math.Round(a.Y, 5) == Math.Round(b.Y, 5)) && (Math.Round(a.Z, 5) == Math.Round(b.Z, 5)))
                return true;
            return false;
        }
    }
}
