using Autodesk.Revit.DB;

namespace ClassLibrary2.Data
{
    public class ColumnData
    {

        //bảng beam object connectivity
        public string Level  { get; set; }
        public string Name { get; set; }
        public string Point_I_ID { get; set; }
        public string Point_J_ID { get; set; }

        public XYZ Point_I { get; set; }
        public XYZ Point_J { get; set; }


        //bảng Concrete Beam Flexure Envelope
        //public double AsTopLongitudinal { get; set; }
        //public double AsBottomLongitudinal { get; set; }


        //tạm thời bỏ qua Insertion point, mục tiêu là sẽ chỉnh đc Insertion point của các cấu kiện để sơ đồ bố trí cấu kiện không chỉ còn là sơ đồ tính

        //bảng section properties
        public string SectionName { get; set; }
        public string ShapeFamily { get; set; }


        //bảng concrete beam reinforcing 
        //public double TopCover { get; set; }
        //public double BottomCover { get; set; }

        //bảng concrete rectangular
        public double b { get; set; } //base
        public double h { get; set; }  //height


        //bảng point object connectivity
        public double XI { get; set; }
        public double YI { get; set; }
        public double ZI { get; set; }
        public double XJ { get; set; }
        public double YJ { get; set; }
        public double ZJ { get; set; }

        

    }
}