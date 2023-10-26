using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document = Autodesk.Revit.DB.Document;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    internal class Filtered_wall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            // Apply the filter to the elements in the active document
            // Use shortcut WhereElementIsNotElementType() to find wall instances only
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList walls = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements() as IList;
            ICollection<ElementId> DSeID = new List<ElementId>();
            String prompt = "The walls in the current document are:\n";
            foreach (Element e in walls)
            {
                prompt += e.Name + " - Id:" + e.Id.ToString() + "\n";
                DSeID.Add(e.Id);
            }
            uidoc.Selection.SetElementIds(DSeID);
            TaskDialog.Show("Revit", prompt);
            return Result.Succeeded;
        }
    }
}
