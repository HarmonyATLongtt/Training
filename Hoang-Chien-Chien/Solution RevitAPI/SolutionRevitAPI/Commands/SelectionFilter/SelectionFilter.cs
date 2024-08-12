using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace SolutionRevitAPI.Commands
{
    internal class SelectionFilter : ISelectionFilter
    {
        private readonly BuiltInCategory allowedCategory;

        public SelectionFilter(BuiltInCategory allowedCategory)
        {
            this.allowedCategory = allowedCategory;
        }

        public BuiltInCategory AllowedCategory => allowedCategory;

        public bool AllowElement(Element elem)
        {
            if (elem.Category != null && elem.Category.BuiltInCategory == allowedCategory)
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