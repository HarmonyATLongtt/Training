using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Data.FrameData
{
    public class AllFrameData
    {
        //bảng frame object connectivity
        public string Level { get; set; }

        public string Name { get; set; }
        public string Point_I_ID { get; set; }
        public string Point_J_ID { get; set; }

        public XYZ Point_I { get; set; }
        public XYZ Point_J { get; set; }

        //tạm thời bỏ qua Insertion point, mục tiêu là sẽ chỉnh đc Insertion point của các cấu kiện để sơ đồ bố trí cấu kiện không chỉ còn là sơ đồ tính

        //bảng section properties
        public string SectionName { get; set; }

        //bảng concrete frame reinforcing
        public double TopCover { get; set; }

        public double BottomCover { get; set; }

        //bảng concrete rectangular
        public double b { get; set; } //base

        public double h { get; set; }  //height

        private List<RebarSetData> RebarSet { get; set; }

        private void foo(List<AllFrameData> elemDatas)
        {
            var rebars = elemDatas.SelectMany(x => x.RebarSet).Where( x=> x.VNStyle == RebarVNStyle.thep_chu).ToList();

        }

        
    }
}