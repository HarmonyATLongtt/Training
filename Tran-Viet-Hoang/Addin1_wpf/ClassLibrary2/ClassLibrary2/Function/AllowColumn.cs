using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ClassLibrary2
{
    internal class AllowColumn : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance inst
                && inst.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column
                && inst.StructuralMaterialType == Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
