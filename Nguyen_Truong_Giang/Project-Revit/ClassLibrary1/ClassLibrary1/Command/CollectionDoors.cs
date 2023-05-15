using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CollectionDoors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            //get Collection
            FilteredElementCollector colection = new FilteredElementCollector(doc);

            //create Filter
            ElementCategoryFilter filterDoor = new ElementCategoryFilter(BuiltInCategory.OST_Doors);

            //Apply Filter to Colection
            IList<Element> doors = colection.WherePasses(filterDoor).WhereElementIsNotElementType().ToElements();

            TaskDialog.Show("Cout Doors: ", doors.Count + "");
            return Result.Succeeded;
        }
    }
}
