using Autodesk.Revit.DB;

namespace ClassLibrary2.Data.FrameData
{
    public class PointData
    {
        public string Id { get; set; }
        public XYZ Point { get; set; }

        public PointData()
        {
            Point = new XYZ();
            Id = string.Empty;
        }
    }
}