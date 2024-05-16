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
    public class CreateGrid : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            using (Transaction t = new Transaction(doc, "Create Grid"))
            {
                t.Start();
                XYZ pointStart = uiDoc.Selection.PickPoint();
                XYZ pointEnd = uiDoc.Selection.PickPoint();

                Line gridLine = Line.CreateBound(pointStart, pointEnd);
                //Tạo Grid
                Grid grid = Grid.Create(doc, gridLine);
                //Đặt tên
                grid.Name = "A";
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}