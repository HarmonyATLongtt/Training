using System;
using System.Collections.Generic;

using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using Autodesk.Revit.UI;
using FirstCommand.View;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateScheduleCommand : IExternalCommand
    {
        private double tolerance = 1e-9;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            CreateNewSchedule(doc);
            return Result.Succeeded;
        }

        //Hàm thực hiện transaction
        public void RunTransaction(Document doc, string transactionName, Action<Transaction> action)
        {
            using (Transaction trans = new Transaction(doc, transactionName))
            {
                trans.Start();

                FailureHandlingOptions options = trans.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new WarningSuppressor());
                trans.SetFailureHandlingOptions(options);

                action(trans); // Thực thi hành động trong Transaction

                trans.Commit();
            }
        }

        //________________
        //Start--Create Schedule

        private void CreateNewSchedule(Document doc)
        {
            RunTransaction(doc, "Create Schedule", (Transaction t) =>
            {
                // 1. Tạo Schedule mới cho Walls
                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Walls));
                // 2. Đặt tên cho Schedule
                schedule.Name = "Thống kê tường";
                // 3. Lấy định nghĩa của Schedule
                ScheduleDefinition definition = schedule.Definition;
                ScheduleFieldId typeId = AddScheduleField(definition, BuiltInParameter.ELEM_TYPE_PARAM, "Loại Tường"); // Loại tường
                ScheduleFieldId areaId = AddScheduleField(definition, BuiltInParameter.HOST_AREA_COMPUTED, "Diện Tích"); // Diện tích
                ScheduleFieldId lengthId = AddScheduleField(definition, BuiltInParameter.CURVE_ELEM_LENGTH, "Chiều Dài"); // Chiều dài

                ScheduleField countField = definition.AddField(ScheduleFieldType.Count);
                countField.ColumnHeading = "Số lượng";

                //6 Sắp xếp (Sorting) theo loại cửa
                ScheduleSortGroupField sortField = new ScheduleSortGroupField(typeId, ScheduleSortOrder.Ascending);

                definition.InsertSortGroupField(sortField, 0);

                // 7 Lọc dữ liệu (Chỉ lấy cửa có chiều rộng > 800mm)
                double lengthInFeet = 800 * 0.00328084;
                ScheduleFilter lengthFilter = new ScheduleFilter(lengthId, ScheduleFilterType.GreaterThan, lengthInFeet);
                definition.AddFilter(lengthFilter);

                // 8 Tính tổng số lượng tường
                definition.IsItemized = false;
                definition.ShowGrandTotal = true; // Hiển thị tổng số lượng của mỗi nhóm
            });
        }

        private ScheduleFieldId AddScheduleField(ScheduleDefinition definition, BuiltInParameter parameter, string columnHeading)
        {
            ScheduleField field = definition.AddField(ScheduleFieldType.Instance, new ElementId(parameter));

            field.ColumnHeading = columnHeading;
            field.IsHidden = false; // Hiện thị cột trên Schedule
            return field.FieldId;
        }

        //End________________
    }
}