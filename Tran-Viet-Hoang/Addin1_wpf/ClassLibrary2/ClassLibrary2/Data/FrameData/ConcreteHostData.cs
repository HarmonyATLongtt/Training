using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace ClassLibrary2.Data.FrameData
{
    public class ConcreteHostData
    {
        public XYZ drawdirection { get; set; }

        public string Level { get; set; }

        public string Name { get; set; }

        public double Length { get; set; }
        public elemDimensionData Dimensions { get; set; }
        public Element Host { get; set; }

        public CoverData Covers { get; set; }
        public PointData StartPoint { get; set; }
        public PointData EndPoint { get; set; }

        public elemReinforcing Reinforcing { get; set; }

        public List<RebarSetData> Rebars { get; set; }

        public ConcreteHostData()
        {
            Rebars = new List<RebarSetData>();
            Dimensions = new elemDimensionData();
            Covers = new CoverData();
            StartPoint = new PointData();
            EndPoint = new PointData();
            Reinforcing = new elemReinforcing();
        }
    }
}