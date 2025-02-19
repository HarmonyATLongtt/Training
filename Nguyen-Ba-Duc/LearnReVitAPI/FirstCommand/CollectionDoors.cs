using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class CollectionDoors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Create collection
            FilteredElementCollector collection = new FilteredElementCollector(doc);

            // Create filter
            ElementCategoryFilter filterDoor = new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            // Apply filter to collection
            IList<Element> doors = collection.WherePasses(filterDoor).WhereElementIsNotElementType().ToElements();

            TaskDialog.Show("Count Door", doors.Count + "");

            return Result.Succeeded;
        }
    }
}