using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Utils;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var floor = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .OfCategory(BuiltInCategory.OST_Floors)
                .Cast<FloorType>()
                .First();

            Line l1 = Line.CreateBound(new XYZ(-10, 10, 0), new XYZ(-10, -10, 0));
            Line l2 = Line.CreateBound(new XYZ(-10, -10, 0), new XYZ(10, -10, 0));
            Line l3 = Line.CreateBound(new XYZ(10, -10, 0), new XYZ(10, 10, 0));
            Arc l4 = Arc.Create(new XYZ(10, 10, 0), new XYZ(-10, 10, 0), new XYZ(0, 20, 0));

            CurveArray curveArray = new CurveArray();
            curveArray.Append(l1);
            curveArray.Append(l2);
            curveArray.Append(l3);
            curveArray.Append(l4);

            try
            {
                using (var trans = new Transaction(doc, "Create new floor"))
                {
                    trans.Start();

                    var floorInf = doc.Create.NewFloor(curveArray, floor, doc.ActiveView.GenLevel, true);
                    new SomeUtils().GetInfor(floorInf);
                    new SomeUtils().SetComments(floorInf, "Some comment was set in here...");

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}