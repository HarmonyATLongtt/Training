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
    public class CreateWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            try
            {
                ///Create Wall
                Transaction t = new Transaction(doc, "Insertar columna");
                t.Start();
                XYZ start = uidoc.Selection.PickPoint();
                XYZ end = uidoc.Selection.PickPoint();
                CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, start, end);
                XYZ poin3 = uidoc.Selection.PickPoint();
                CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, end, poin3);
                XYZ poin4 = uidoc.Selection.PickPoint();
                CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin3, poin4);
                XYZ poin5 = uidoc.Selection.PickPoint();
                CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, poin4, poin5);
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

        public Wall CreateWallUsingCurve1(Autodesk.Revit.DB.Document document, Level level, UIDocument uidoc, XYZ start, XYZ end)
        {
            // Build a location line for the wall creation

            Line geomLine = Line.CreateBound(start, end);
            // Create a wall using the location line
            return Wall.Create(document, geomLine, level.Id, true);
        }
    }
}