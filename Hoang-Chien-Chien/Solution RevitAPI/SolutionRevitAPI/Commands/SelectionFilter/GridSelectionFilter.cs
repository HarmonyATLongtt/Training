using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace SolutionRevitAPI.Commands
{
    internal class GridSelectionFilter : ISelectionFilter
    {
        private readonly BuiltInCategory allowedCategory;

        public GridSelectionFilter(BuiltInCategory allowedCategory)
        {
            this.allowedCategory = allowedCategory;
        }

        public BuiltInCategory AllowedCategory => allowedCategory;

        public bool AllowElement(Element elem)
        {
            if (elem.Category != null && elem.Category.BuiltInCategory == BuiltInCategory.OST_Grids)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}