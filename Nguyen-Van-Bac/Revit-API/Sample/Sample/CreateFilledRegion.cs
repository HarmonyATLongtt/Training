using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateFilledRegion : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            // Bắt đầu một giao dịch
            using (Transaction trans = new Transaction(doc, "Create Filled Region"))
            {
                trans.Start();

                // Lấy kiểu FilledRegionType có sẵn trong tài liệu
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                FilledRegionType filledRegionType = collector.OfClass(typeof(FilledRegionType)).Cast<FilledRegionType>()
                    .FirstOrDefault();

                if (filledRegionType == null)
                {
                    message = "Không tìm thấy FilledRegionType nào trong tài liệu.";
                    return Result.Failed;
                }

                List<Curve> curves = new List<Curve>();
                XYZ point1 = new XYZ(0, 0, 0);
                XYZ point2 = new XYZ(10, 0, 0);
                XYZ point3 = new XYZ(10, 10, 0);
                XYZ point4 = new XYZ(0, 10, 0);

                curves.Add(Line.CreateBound(point1, point2));
                curves.Add(Line.CreateBound(point2, point3));
                curves.Add(Line.CreateBound(point3, point4));
                curves.Add(Line.CreateBound(point4, point1));
                if (curves.Count == 0)
                {
                    TaskDialog.Show("Error", "cannot create curvers");
                    return Result.Failed;
                }
                CurveLoop curveLoop = CurveLoop.Create(curves);

                // Tạo danh sách các CurveLoop
                List<CurveLoop> loopList = new List<CurveLoop> { curveLoop };
                if (loopList.Count == 0 || loopList.Any(loop => loop == null || !loop.Any()))
                {
                    message = "CurveLoop list is empty or invalid.";
                    return Result.Failed;
                }
                // Tạo FilledRegion
                View view = doc.ActiveView;
                FilledRegion filledRegion = FilledRegion.Create(doc, filledRegionType.Id, view.Id, loopList);

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}