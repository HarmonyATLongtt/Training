#region Namespaces

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace ExRevitAPI
{
    [Transaction(TransactionMode.Manual)]
    public class FilterInstance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Access current selection

            Selection sel = uidoc.Selection;

            // create collector
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> allElements = collector.WhereElementIsNotElementType().ToElements();

            List<Category> listCate = new List<Category>();

            var listKhongTrungLapID = allElements.GroupBy(x => x.Category).Select(g => g.Key).ToList();
            var listKhongTrungLapID2 = allElements.Select(g => g.Category).Distinct().Cast<Category>().ToList();

            return Result.Succeeded;
        }
    }
}