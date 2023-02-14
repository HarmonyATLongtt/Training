using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace ConcreteFacing.Common
{
    internal class AllowBeamAndColumn : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance inst
                && (inst.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column || inst.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam)
                && inst.StructuralMaterialType == Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}