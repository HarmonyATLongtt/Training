using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaiTap_Revit
{
    [Transaction(TransactionMode.Manual)]
    internal class CreateFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            double elevation = 5.0;

            try
            {
                using (Transaction tx = new Transaction(doc, "Create Floor"))
                {
                    tx.Start();

                    // Lấy loại sàn mặc định từ project
                    ElementId floorTypeId = Floor.GetDefaultFloorType(doc, false);

                    // Lấy level gần nhất với độ cao đã cấp
                    double offset;
                    ElementId levelId = Level.GetNearestLevelId(doc, elevation, out offset);

                    // Yêu cầu người dùng chọn điểm
                    List<XYZ> points = new List<XYZ>();
                    while (true)
                    {
                        XYZ point = uidoc.Selection.PickPoint("Please pick a point for the floor (right-click to finish)");
                        points.Add(point);

                        if (points.Count >= 3)
                        {
                            TaskDialogResult result = TaskDialog.Show("Continue", "Do you want to pick more points?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                            if (result == TaskDialogResult.No)
                            {
                                break;
                            }
                        }
                    }

                    // Kiểm tra số lượng điểm
                    if (points.Count < 3)
                    {
                        TaskDialog.Show("Error", "You must select at least 3 points to create a floor.");
                        return Result.Failed;
                    }

                    // Xây dựng profile sàn cho việc tạo sàn
                    CurveLoop profile = new CurveLoop();
                    for (int i = 0; i < points.Count; i++)
                    {
                        XYZ start = points[i];
                        XYZ end = points[(i + 1) % points.Count]; // Kết nối điểm cuối với điểm đầu
                        profile.Append(Line.CreateBound(start, end));
                    }

                    // Tạo sàn
                    var floor = Floor.Create(doc, new List<CurveLoop> { profile }, floorTypeId, levelId);

                    tx.Commit();
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