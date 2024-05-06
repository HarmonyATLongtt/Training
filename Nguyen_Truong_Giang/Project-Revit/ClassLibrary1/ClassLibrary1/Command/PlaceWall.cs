using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class PlaceWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            //get Level 1
            FilteredElementCollector collecter = new FilteredElementCollector(doc);
            Level level = collecter.OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .First(x => x.Name == "Level 1");

            //Create point
            XYZ p1 = new XYZ(0,0,0);
            XYZ p2 = new XYZ(10,0,0);

            //Create Line
            Line line = Line.CreateBound(p1, p2);

            try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();
                    Wall.Create(doc, line, level.Id,false);
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}