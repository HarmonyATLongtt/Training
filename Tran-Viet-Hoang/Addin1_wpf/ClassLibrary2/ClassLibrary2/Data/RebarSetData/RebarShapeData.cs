using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;

namespace ClassLibrary2.Data
{
    public class RebarShapeData
    {
        public Dictionary<string, double> Segments { get; set; }

        public RebarShape Shape { get; set; }

        public RebarShapeData()
        {
            Segments = new Dictionary<string, double>();
        }

        public RebarShapeData(RebarShape shape, Dictionary<string, double> segments)
        {
            Shape = shape;
            Segments = segments;
        }
    }
}