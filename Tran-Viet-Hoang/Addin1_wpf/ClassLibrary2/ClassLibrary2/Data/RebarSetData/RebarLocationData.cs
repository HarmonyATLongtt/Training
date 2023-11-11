using Autodesk.Revit.DB;

namespace ClassLibrary2.Data
{
    public class RebarLocationData
    {
        public XYZ RebarOrigin { get; set; }
        public BoundingBoxXYZ BoundingBox { get; set; }

        public RebarLocationData()
        {
        }
    }
}