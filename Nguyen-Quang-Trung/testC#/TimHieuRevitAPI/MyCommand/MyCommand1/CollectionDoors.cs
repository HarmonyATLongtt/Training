using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyCommand1
{
    [TransactionAttribute(TransactionMode.ReadOnly)]  // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class CollectionDoors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;

            // create collection
            FilteredElementCollector collection = new FilteredElementCollector(doc);

            // create filter
            ElementCategoryFilter filterDoor = new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            // use filter in collection
            IList<Element> doors = collection.WherePasses(filterDoor).WhereElementIsNotElementType().ToElements();

            TaskDialog.Show("Door Count", doors.Count + "");
            return Result.Succeeded;
        }
    }
}