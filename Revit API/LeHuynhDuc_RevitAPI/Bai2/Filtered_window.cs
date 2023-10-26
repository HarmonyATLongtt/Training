using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    internal class Filtered_window : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector colector = new FilteredElementCollector(doc);
            IList Windows = colector.WherePasses(filter).WhereElementIsNotElementType().ToElements() as IList;
            String prompt = "The Windows in the current document are:\n";
            ICollection<ElementId> DSeID = new List<ElementId>();
            foreach (Element w in Windows)
            {
                prompt += w.Name + " - Id:" + w.Id.ToString() + "\n";
                DSeID.Add(w.Id);
            }
            uidoc.Selection.SetElementIds(DSeID);
            TaskDialog.Show("Filter_Windows",prompt);
            return Result.Succeeded;
        }
    }
}
