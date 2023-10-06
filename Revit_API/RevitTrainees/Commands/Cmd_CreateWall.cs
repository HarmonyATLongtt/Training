using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Utils;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateWall : IExternalCommand
    {
        [System.Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var wall = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .WhereElementIsElementType()
                        .Cast<WallType>()
                        .First();

            XYZ point1 = new XYZ(UnitUtils.ConvertToInternalUnits(-10, DisplayUnitType.DUT_METERS), 0, 0);
            XYZ point2 = new XYZ(UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_METERS), 0, 0);
            Line line = Line.CreateBound(point1, point2);
            try
            {
                using (var trans = new Transaction(doc, "Create new wall"))
                {
                    trans.Start();

                    var wallInf = Wall.Create(doc, line, wall.Id, doc.ActiveView.GenLevel.Id, 100, 0, false, true);
                    new SomeUtils().GetInfor(wallInf);
                    new SomeUtils().SetComments(wallInf, "Some comment was set in here...");

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}