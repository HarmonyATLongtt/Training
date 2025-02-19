using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Get level
            Level level = new FilteredElementCollector(doc)
                   .OfClass(typeof(Level))
                   .Cast<Level>()
                   .FirstOrDefault();
            if (level == null)
            {
                TaskDialog.Show("Error", "Không tìm thấy Level trong dự án.");
                return Result.Failed;
            }
            WallType wallType = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType))
                .Cast<WallType>()
                .FirstOrDefault();
            if (wallType == null)
            {
                TaskDialog.Show("Error", "Không tìm thấy WallType.");
                return Result.Failed;
            }
            // Định nghĩa đường cơ sở cho tường (Line)
            // Create point
            XYZ p1 = new XYZ(0, 0, 0);
            XYZ p2 = new XYZ(10, 0, 0);

            // Create line
            Line line = Line.CreateBound(p1, p2);

            try
            {
                using (Transaction trans = new Transaction(doc, "Create wall"))
                {
                    trans.Start();

                    //Wall wall = Wall.Create(doc, new List<Curve> { line }, wallType.Id, level.Id, false);
                    Wall wall = Wall.Create(doc, line, wallType.Id, level.Id, 10.0, 0.0, false, false);

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}