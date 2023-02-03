using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ClassLibrary2.Data.FrameData
{
    public class ConcreteHostData
    {
        #region old

        //bảng frame object connectivity
        //public string Level { get; set; }

        //public string Name { get; set; }
        //public string Point_I_ID { get; set; }
        //public string Point_J_ID { get; set; }

        //public XYZ Point_I { get; set; }
        //public XYZ Point_J { get; set; }

        //tạm thời bỏ qua Insertion point, mục tiêu là sẽ chỉnh đc Insertion point của các cấu kiện để sơ đồ bố trí cấu kiện không chỉ còn là sơ đồ tính

        //bảng section properties
        //public string SectionName { get; set; }

        //bảng concrete frame reinforcing
        //public double TopCover { get; set; }

        //public double BottomCover { get; set; }

        //bảng concrete rectangular
        //public double b { get; set; } //base

        //public double h { get; set; }  //height

        //public double length { get; set; }

        public XYZ drawdirection { get; set; }
        public RebarSetData HostRebar { get; set; }

        #endregion old

        #region new

        public string Level { get; set; }

        public string Name { get; set; }

        public double Length { get; set; }
        public elemDimensionData Dimensions { get; set; }

        public CoverData Covers { get; set; }
        public PointData StartPoint { get; set; }
        public PointData EndPoint { get; set; }

        public elemReinforcing Reinforcing { get; set; }

        public List<RebarSetData> Standards { get; set; }
        public RebarSetData Stirrup_Tie { get; set; }

        #endregion new

        public ConcreteHostData()
        {
            HostRebar = new RebarSetData();

            Dimensions = new elemDimensionData();
            Covers = new CoverData();
            StartPoint = new PointData();
            EndPoint = new PointData();
            Reinforcing = new elemReinforcing();
            Standards = new List<RebarSetData>();
            Stirrup_Tie = new RebarSetData();
        }
    }
}