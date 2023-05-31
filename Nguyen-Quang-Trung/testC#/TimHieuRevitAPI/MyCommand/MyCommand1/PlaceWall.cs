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
    [TransactionAttribute(TransactionMode.Manual)] // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class PlaceWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;

            //find family
            FilteredElementCollector collection = new FilteredElementCollector(doc);
            Level level = collection.OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .First(x => x.Name == "Level 1");

            // create point
            XYZ p1 = new XYZ(0, 0, 0);
            XYZ p2 = new XYZ(15, 0, 0);

            // create line
            Line line = Line.CreateBound(p1, p2);

            try
            {
                using (Transaction trans = new Transaction(doc, "Place family"))
                {
                    trans.Start();
                    Wall.Create(doc, line, level.Id, false);
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}