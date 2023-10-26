using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CmdAdd_View_Sheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //tạo SheetView
            ViewSheet viewSheet = new FilteredElementCollector(doc)
                                  .OfCategory(BuiltInCategory.OST_Sheets)
                                  .Cast<ViewSheet>()
                                  .First(x => x.Name == "New sheet");
            //lấy view 3d cần đặt vào sheet view
            Element view = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_Views)
                            .Cast<View>()
                            .First(x => x.Name == "{3D}");
            //Lấy tọa độ điểm ở giữa sheet view để đặt view3d
            UV location = new UV((viewSheet.Outline.Max.U + viewSheet.Outline.Min.U) / 2,
                                  (viewSheet.Outline.Max.V + viewSheet.Outline.Min.V) / 2);

            using (Transaction trans = new Transaction(doc, "Add sheet"))
            {
                trans.Start();
                //Tạo view port
                Viewport.Create(doc, viewSheet.Id, view.Id, new XYZ(location.U, location.V, 0));
                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
}
