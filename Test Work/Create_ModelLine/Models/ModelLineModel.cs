using Autodesk.Revit.DB;

namespace Create_ModelLine.Models
{
    public class ModelLineModel
    {
        private string _name;
        private ElementId _lineStyle;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public ElementId LineStyle
        {
            get =>  _lineStyle;

            set { _lineStyle = value; }
        }
    }
}