using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

namespace CreateColumnApi
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WallCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            Selection selection = uiApp.ActiveUIDocument.Selection;
            XYZ startPoint = null;
            XYZ endPoint = null;

            try
            {
                startPoint = selection.PickPoint("Pick start point...");
                endPoint = selection.PickPoint("Pick end point...");
            }
            catch
            {

            }

            FilteredElementCollector wallTypes
                = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType));

            WallType wallType = wallTypes.Cast<WallType>()
                .First();
            //.FirstOrDefault(e => e.Id.IntegerValue == 6291);

            FilteredElementCollector filterLevel = new FilteredElementCollector(doc);
            filterLevel.WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level));

            Level firstLevel = filterLevel.FirstElement() as Level;

            using (var transaction = new Transaction(doc))
            {
                transaction.Start("Create Wall");

                try
                {
                    // Create line
                    Line geomLine = Line.CreateBound(startPoint, endPoint);
                    // Create wall
                    Element element = Wall.Create(doc, geomLine, wallType.Id, firstLevel.Id, 300, 0, false, true);
                    Parameter eleParam = element.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                    eleParam.Set(6);
                }

                catch (Exception ex)
                {
                    TaskDialog.Show("Message", ex.Message);
                }

                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}
