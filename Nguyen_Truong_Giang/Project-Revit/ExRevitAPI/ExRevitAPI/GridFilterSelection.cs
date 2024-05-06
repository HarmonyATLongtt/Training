using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExRevitAPI
{
    class GridFilterSelection : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem != null && elem is Grid;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
