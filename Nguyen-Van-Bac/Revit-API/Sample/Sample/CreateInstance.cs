using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateInstance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                //string columnName = "W10X49";
                //// Tìm FamilySymbol tương ứng với kiểu họ cột muốn chèn
                //FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));
                //FamilySymbol columnType = col.FirstOrDefault(e => e.Name.Equals(columnName)) as FamilySymbol;

                //// Xác minh rằng FamilySymbol tương ứng đã được tìm thấy
                //if (columnType == null)
                //{
                //    TaskDialog.Show("Error", "Không tìm thấy loại họ cột được chỉ định.");

                //}

                ////Nhận điểm chèn cột bằng đối tượng lựa chọn của người dùng
                //XYZ insertionPoint = uidoc.Selection.PickPoint();

                //Transaction transaction = new Transaction(doc, "Insertar columna");
                //transaction.Start();
                //FamilyInstance columnInstance = doc.Create.NewFamilyInstance(insertionPoint, columnType, doc.ActiveView.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                //transaction.Commit();
                //TaskDialog.Show("Đã chèn cột", "Một cột mới đã được chèn vào chế độ views 3D để xem.");
                ///Create Wall
                //Transaction t = new Transaction(doc, "Insertar columna");
                //t.Start();
                //XYZ start = uidoc.Selection.PickPoint();
                //XYZ end = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, start, end);
                //XYZ poin3 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, end, poin3);
                //XYZ poin4 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin3, poin4);
                //XYZ poin5 = uidoc.Selection.PickPoint();
                //CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin4, poin5);
                //t.Commit();
                //TaskDialog.Show("Đã thêm tường", "Một tường mới đã được chèn vào chế độ views 3D để xem.");
                ///Create Floor
                Transaction t = new Transaction(doc, "Insertar columna");
                t.Start();
                CreateFloorAtElevation(doc.ActiveView.GenLevel, doc, uidoc);
                t.Commit();
                TaskDialog.Show("Đã thêm tường", "Một tường mới đã được chèn vào chế độ views 3D để xem.");
                return Result.Succeeded;
            }
            catch
            {
                message = "Unexpected Exception thrown.";
                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        public Wall CreateWallUsingCurve1(Autodesk.Revit.DB.Document document, Level level, UIDocument uidoc, XYZ start, XYZ end)
        {
            // Build a location line for the wall creation

            Line geomLine = Line.CreateBound(start, end);
            // Create a wall using the location line

            return Wall.Create(document, geomLine, level.Id, true);
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