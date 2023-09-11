using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace CreateColumnApi
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FloorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Level level = doc.ActiveView.GenLevel;

            float elevation = (float)level.Elevation;

            XYZ first = new XYZ(0, 0, elevation);
            XYZ second = new XYZ(0, 20, elevation);
            XYZ third = new XYZ(10, 20, elevation);
            XYZ fourth = new XYZ(10, 0, elevation);

            CurveArray profile = new CurveArray();
            profile.Append(Line.CreateBound(first, second));
            profile.Append(Line.CreateBound(second, third));
            profile.Append(Line.CreateBound(third, fourth));
            profile.Append(Line.CreateBound(fourth, first));

            FloorType floorType = new FilteredElementCollector(doc).OfClass(typeof(FloorType)).Cast<FloorType>().First();

            using (var transaction = new Transaction(doc, "Create Floor"))
            {
                transaction.Start();

                try
                {
                    doc.Create.NewFloor(profile, floorType, level, true);
                }
                catch
                {

                }
                transaction.Commit();
            }


            return Result.Succeeded;
        }
    }
}
