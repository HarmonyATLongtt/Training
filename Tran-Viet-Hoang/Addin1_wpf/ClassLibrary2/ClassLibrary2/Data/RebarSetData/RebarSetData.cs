using Autodesk.Revit.DB.Structure;

namespace ClassLibrary2.Data
{
    public class RebarSetData
    {
        public RebarDiameterData DiameterData { get; set; }
        public RebarHostData HostData { get; set; }
        public RebarLayoutData LayoutData { get; set; }
        public RebarShapeData ShapeData { get; set; }
        public RebarStyleData StyleData { get; set; }

        public RebarLocationData LocationData { get; set; }
        public Rebar ColumnStirrup { get; set; }
        public Rebar BeamStirrup { get; set; }

        public RebarStyle standard { get; set; }

        public RebarSetData()
        {
            standard = RebarStyle.Standard;
        }
    }
}