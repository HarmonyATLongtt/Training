using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace ClassLibrary2
{
    internal class AllowColumn : ISelectionFilter
    {
        public bool AllowElement(Element srtcolumnelem)
        {
            return srtcolumnelem is FamilyInstance inst
                && inst.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column
                && inst.StructuralMaterialType == Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}