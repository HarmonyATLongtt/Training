using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ClassLibrary2.Data.FrameData
{
    public class ConcreteHostData
    {
        public string Level { get; set; }
        public string Name { get; set; }
        public double Length { get; set; }
        public XYZ drawdirection { get; set; }
        public ElemDimensionData Dimensions { get; set; }
        public Element Host { get; set; }
        public CoverData Covers { get; set; }
        public PointData StartPoint { get; set; }
        public PointData EndPoint { get; set; }
        public ElemReinforcing Reinforcing { get; set; }
        public List<RebarSetData> Rebars { get; set; }
        public ConcreteHostData()
        {
            Rebars = new List<RebarSetData>();
            Dimensions = new ElemDimensionData();
            Covers = new CoverData();
            StartPoint = new PointData();
            EndPoint = new PointData();
            Reinforcing = new ElemReinforcing();
        }
    }
}