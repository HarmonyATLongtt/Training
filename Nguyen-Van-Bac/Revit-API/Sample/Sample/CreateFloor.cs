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
    public class CreateFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            try
            {
                ///Create floor
                Transaction t = new Transaction(doc, "Insertar columna");
                t.Start();
                CreateFloorAtElevation(doc.ActiveView.GenLevel, doc, uidoc);
                t.Commit();
                TaskDialog.Show("Đã thêm tường", "Một tường mới đã được chèn vào chế độ views 3D để xem.");
            }
            catch
            {
                message = "Unexpected Exception thrown.";
                return Autodesk.Revit.UI.Result.Failed;
            }
            return Result.Succeeded;
        }

        public Floor CreateFloorAtElevation(Level level, Document document, UIDocument uidoc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(FloorType));
            FloorType floorType = collector.FirstElement() as FloorType;
            XYZ first = uidoc.Selection.PickPoint();
            XYZ second = uidoc.Selection.PickPoint();
            XYZ third = uidoc.Selection.PickPoint();
            XYZ fourth = uidoc.Selection.PickPoint();
            CurveLoop profile = new CurveLoop();
            profile.Append(Line.CreateBound(first, second));
            profile.Append(Line.CreateBound(second, third));
            profile.Append(Line.CreateBound(third, fourth));
            profile.Append(Line.CreateBound(fourth, first));

            return Floor.Create(document, new List<CurveLoop> { profile }, floorType.Id, level.Id, true, null, 0.0);
        }
    }
}