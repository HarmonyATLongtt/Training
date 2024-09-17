using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BaiTap_Revit
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Yêu cầu người dùng chọn điểm
                List<XYZ> points = new List<XYZ>();
                while (true)
                {
                    XYZ point = uidoc.Selection.PickPoint("Please pick a point (right-click to finish)");
                    points.Add(point);

                    if (points.Count >= 2)
                    {
                        TaskDialogResult result = TaskDialog.Show("Continue", "Do you want to pick more points?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                        if (result == TaskDialogResult.No)
                        {
                            break;
                        }
                    }
                }

                // Kiểm tra số lượng điểm
                if (points.Count < 2)
                {
                    TaskDialog.Show("Error", "You must pick at least 2 points to create walls.");
                    return Result.Failed;
                }

                // Yêu cầu người dùng nhập chiều cao của tường
                double wallHeight = 10.0; // Default height, you can prompt the user for this

                using (Transaction trans = new Transaction(doc, "Create Walls"))
                {
                    trans.Start();

                    // Get a wall type (assuming it's the first one)
                    WallType wallType = new FilteredElementCollector(doc)
                        .OfClass(typeof(WallType))
                        .FirstOrDefault() as WallType;

                    if (wallType == null)
                    {
                        message = "No wall type found in the project.";
                        return Result.Failed;
                    }

                    // Lấy level từ view hiện tại
                    Level level = doc.ActiveView.GenLevel;

                    if (level == null)
                    {
                        message = "Active view does not have a valid level.";
                        return Result.Failed;
                    }

                    // Tạo các tường giữa các điểm liên tiếp
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        XYZ startPoint = points[i];
                        XYZ endPoint = points[i + 1];

                        // Tạo một Line từ điểm bắt đầu và kết thúc
                        Line wallLine = Line.CreateBound(startPoint, endPoint);

                        // Tạo tường
                        Wall wall = Wall.Create(doc, wallLine, wallType.Id, level.Id, wallHeight, 0, false, false);
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}