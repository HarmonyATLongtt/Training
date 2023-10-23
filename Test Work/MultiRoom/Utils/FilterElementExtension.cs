using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;

namespace MultiRoom.Utils
{
    public class FilterElementExtension : ISelectionFilter
    {
        private readonly Func<Element, bool> validateElement;

        public FilterElementExtension(Func<Element, bool> validateElement)
        {
            this.validateElement = validateElement;
        }

        public bool AllowElement(Element elem)
        {
            return validateElement(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}