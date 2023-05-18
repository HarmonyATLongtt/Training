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

            // Đếm số lượng category
            HashSet<string> categories = new HashSet<string>();
            foreach (Element element in allElements)
            {
                categories.Add(element.Name);
            }
            int categoryCount = categories.Count;

            // Đếm số lượng family
            HashSet<string> families = new HashSet<string>();
            foreach (Element element in allElements)
            {
                families.Add(element.Name);
            }
            int familyCount = families.Count;

            // Đếm số lượng type
            int typeCount = allElements.Select(element => element.GetTypeId()).Distinct().Count();

            // Đếm số lượng instance
            int instanceCount = allElements.Count;

            // In ra thông tin
            TaskDialog.Show("Instance Information",
                "Số lượng Category: " + categoryCount.ToString() + "\n" +
                "Số lượng Family: " + familyCount.ToString() + "\n" +
                "Số lượng Type: " + typeCount.ToString() + "\n" +
                "Số lượng Instance: " + instanceCount.ToString());

            return Result.Succeeded;
        }
    }
}