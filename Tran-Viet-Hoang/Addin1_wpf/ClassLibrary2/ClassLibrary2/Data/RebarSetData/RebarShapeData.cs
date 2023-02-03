using Autodesk.Revit.DB.Structure;

namespace ClassLibrary2.Data
{
    public class RebarShapeData
    {
        public double B { get; set; }
        public double C { get; set; }
        public double D { get; set; }
        public double E { get; set; }

        public RebarShape Shape { get; set; }

        public RebarShapeData()
        {
        }
    }
}