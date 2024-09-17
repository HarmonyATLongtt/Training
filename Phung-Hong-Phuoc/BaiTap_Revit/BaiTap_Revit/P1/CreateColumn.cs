using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaiTap_Revit
{
    [Transaction(TransactionMode.Manual)]
    internal class CreateColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Tạo hộp thoại với các nút tùy chỉnh
                TaskDialog dialog = new TaskDialog("Chọn Loại Cột");
                dialog.MainInstruction = "Chọn loại cột bạn muốn tạo:";
                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Cột Đứng");
                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Cột Nghiêng");

                // Hiển thị hộp thoại và lấy kết quả
                TaskDialogResult result = dialog.Show();

                // Khai báo điểm gốc và điểm đỉnh của cột
                XYZ basePoint = null;
                XYZ topPoint = null;

                if (result == TaskDialogResult.CommandLink1)
                {
                    // Tạo cột đứng
                    basePoint = uidoc.Selection.PickPoint("Vui lòng chọn điểm gốc của cột");

                    // Đặt điểm đỉnh của cột đứng cách điểm gốc một khoảng cố định (10 đơn vị ở đây)
                    topPoint = new XYZ(basePoint.X, basePoint.Y, basePoint.Z + 10); // Chiều cao tùy ý cho ví dụ
                }
                else if (result == TaskDialogResult.CommandLink2)
                {
                    // Tạo cột nghiêng
                    basePoint = uidoc.Selection.PickPoint("Vui lòng chọn điểm gốc của cột");
                    topPoint = uidoc.Selection.PickPoint("Vui lòng chọn điểm đỉnh của cột");
                }
                else
                {
                    // Nếu người dùng đóng hộp thoại hoặc chọn nút khác
                    message = "Người dùng đã hủy thao tác.";
                    return Result.Cancelled;
                }

                using (Transaction trans = new Transaction(doc, "Create Column"))
                {
                    trans.Start();

                    // Lấy loại cột từ danh sách các FamilySymbol
                    FamilySymbol columnType = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_StructuralColumns)
                        .FirstOrDefault() as FamilySymbol;

                    if (columnType == null)
                    {
                        // Nếu không tìm thấy loại cột, hiển thị thông báo lỗi
                        message = "Không tìm thấy loại cột nào.";
                        return Result.Failed;
                    }

                    // Đảm bảo loại cột đang được kích hoạt
                    if (!columnType.IsActive)
                    {
                        columnType.Activate();
                        doc.Regenerate();
                    }

                    if (result == TaskDialogResult.CommandLink1)
                    {
                        // Tạo cột đứng
                        Level baseLevel = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level))
                            .FirstElement() as Level;

                        doc.Create.NewFamilyInstance(basePoint, columnType, baseLevel, StructuralType.Column);
                    }
                    else
                    {
                        // Tạo cột nghiêng
                        Line columnLine = Line.CreateBound(basePoint, topPoint);

                        Level baseLevel = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level))
                            .FirstElement() as Level;

                        FamilyInstance slantedColumn = doc.Create.NewFamilyInstance(
                            columnLine, columnType, baseLevel, StructuralType.Column);

                        // Thiết lập khoảng cách từ cột đến các level nếu cần
                        double baseOffset = 0.0;
                        double topOffset = 0.0;

                        Parameter topOffsetPara = slantedColumn.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM);
                        topOffsetPara.Set(UnitUtils.ConvertToInternalUnits(topOffset, topOffsetPara.GetUnitTypeId()));
                        Parameter baseOffsetPara = slantedColumn.get_Parameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM);
                        baseOffsetPara.Set(UnitUtils.ConvertToInternalUnits(baseOffset, topOffsetPara.GetUnitTypeId()));
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