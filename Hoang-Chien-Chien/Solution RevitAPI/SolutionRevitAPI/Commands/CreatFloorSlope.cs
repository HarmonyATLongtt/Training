using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.WPF.Model;
using SolutionRevitAPI.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreatFloorSlope : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<XYZ> Points = new List<XYZ>();
            try
            {
                try
                {
                    while (true)
                    {
                        // Yêu cầu người dùng chọn một điểm
                        XYZ point = uidoc.Selection.PickPoint("Chọn một điểm");
                        // Thêm điểm vào danh sách
                        Points.Add(point);

                        TaskDialogResult result = TaskDialog.Show("Tiếp tục", "Bạn có muốn chọn thêm điểm không?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);
                        if (result == TaskDialogResult.No)
                        {
                            break;
                        }
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    Points.Clear();
                    return Result.Cancelled;
                }

                if (Points.Count() < 3)
                {
                    TaskDialog.Show("Warning", "Không thể tạo tường ít hơn 3 điểm, vui lòng chọn lại!");
                }
                else
                {
                    using (Transaction trans = new Transaction(doc, "Creat floor slope"))
                    {
                        trans.Start();
                        CurveLoop profile = new CurveLoop();
                        for (int i = 0; i < Points.Count() - 1; i++)
                        {
                            profile.Append(Line.CreateBound(Points[i], Points[i + 1]));
                        }
                        profile.Append(Line.CreateBound(Points.Last(), Points.First()));
                        var floorStyle = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Floors)
                                   .OfClass(typeof(FloorType))
                                   .WhereElementIsElementType()
                                   .Cast<FloorType>()
                                   .ToList().FirstOrDefault();
                        var level = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().Cast<Level>().ToList().FirstOrDefault();

                        
                        if (floorStyle != null)
                        {
                            var line = Line.CreateBound(Points.Last(), Points.First());
                            Floor floor = Floor.Create(doc, new List<CurveLoop> { profile }, floorStyle.Id, level.Id, false, line, 0.5);

                        }
                        trans.Commit();
                        
                    }

                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}