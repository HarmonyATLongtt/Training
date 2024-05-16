using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateLevel : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            // Đặt độ cao cho Level mới
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var level = collector.OfClass(typeof(Level)).Cast<Level>().ToList();
            if (level.Count == 0)
            {
                message = "No Level found in the document";
                return Result.Failed;
            }
            //lấy level cao nhất
            Level highesLevel = level.OrderByDescending(l => l.Elevation).FirstOrDefault();
            double newElevation = highesLevel.Elevation + 10.0;
            using (Transaction trans = new Transaction(doc, "Create new  Level"))
            {
                trans.Start();

                // Tạo Level mới
                Level newLevel = Level.Create(doc, newElevation);

                // Đặt tên cho Level mới
                newLevel.Name = "New Level 3";

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}