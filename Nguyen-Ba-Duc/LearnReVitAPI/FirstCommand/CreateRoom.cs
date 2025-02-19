using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateRoom : IExternalCommand
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
            // Điểm trung tâm nằm trong ranh giới phòng
            XYZ roomPoint = new XYZ(5, 5, 0);

            try
            {
                using (Transaction trans = new Transaction(doc, "Create room"))
                {
                    trans.Start();

                    Room room = doc.Create.NewRoom(level, new UV(roomPoint.X, roomPoint.Y));

                    if (room == null)
                    {
                        TaskDialog.Show("Error", "Không thể tạo phòng tại vị trí này.");
                        return Result.Failed;
                    }
                    // Gán tên và số cho phòng
                    room.Name = "Phong Ngu";
                    room.Number = "101";

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