using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ClassLibrary2.Data
{
    public class RebarSetData
    {
        public double Type { get; set; } //diameter : đường kính
        public int Number { get; set; } // số thanh
        public double Spacing { get; set; } //khoảng cách giữa các thanh
        public double RebarCrossSectionArea { get; set; } //tổng diện tích mặt cắt thép
        public double CrossSectionWidth { get; set; } // bề rộng mặt cắt rải thép
        public ElementId HostID { get; set;}

        public Rebar ColumnStirrup { get; set; }
        public Rebar BeamStirrup { get; set; }
        public XYZ BeamStirrupOrigin { get; set; }
        public XYZ BeamStandardOrigin { get; set; } //điểm origin cho thép dọc
        public double HostLength { get; set; }
        public double Host_b { get; set; }
        public double Host_h { get; set; }

        public BoundingBoxXYZ Host_boundingbox_1 { get; set; }
        public BoundingBoxXYZ Host_boundingbox_2 { get; set; }
        public RebarStyle Style { get; set; }
        public RebarVNStyle VNStyle { get; set; }
        Rebar Rebar { get; set; }
    }
    public enum RebarVNStyle
    {
        thep_chu,
        thep_dai_dau,
        thep_dai_giua,
        thep_dai_cuoi,
        thep_chong_phinh,
        thep_dia,

    }
}
