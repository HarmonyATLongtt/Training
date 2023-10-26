using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    internal class Create_Region : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            List<CurveLoop> curveLoopList = new List<CurveLoop>();

            // Tạo một CurveLoop
            CurveLoop curveLoop = new CurveLoop();

            // Thêm các đường cong vào CurveLoop
            curveLoop.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));
            curveLoop.Append(Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 10, 0)));
            curveLoop.Append(Line.CreateBound(new XYZ(10, 10, 0), new XYZ(0, 10, 0)));
            curveLoop.Append(Line.CreateBound(new XYZ(0, 10, 0), new XYZ(0, 0, 0)));

            // Thêm CurveLoop vào danh sách
            curveLoopList.Add(curveLoop);

            // Tạo một đối tượng FilledRegionType
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FilledRegionType));
            FilledRegionType filledRegionType = collector.FirstElement() as FilledRegionType;
            using (Transaction trans = new Transaction(doc, "Create_Region"))
            {
                trans.Start();
                FilledRegion region = FilledRegion.Create(doc, filledRegionType.Id, doc.ActiveView.Id, curveLoopList);
                trans.Commit();
            }
            return Result.Succeeded;
        }
        
    }
}
