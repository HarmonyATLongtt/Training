using Autodesk.Revit.DB;

namespace ClassLibrary2.Data
{
    public class RebarHostData
    {
        public ElementId HostID { get; set; }

        public FamilyInstance Host { get; set; }

        public double HostLength { get; set; }
        public double Host_b { get; set; }
        public double Host_h { get; set; }

        public RebarHostData()
        {
        }
    }
}