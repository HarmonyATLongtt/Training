using Autodesk.Revit.DB;

namespace CreateFilterForEachView.Models
{
    public class ViewModel
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private View _viewSelect;

        public View ViewSelect
        {
            get { return _viewSelect; }
            set { _viewSelect = value; }
        }
    }
}