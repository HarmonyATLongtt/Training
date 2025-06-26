using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using Autodesk.Revit.UI;
using FirstCommand.View;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.ExtensibleStorage;
using FirstCommand.Support;

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
            Application app = uiapp.Application;

            CreateSharedParameter(app, doc);
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

        private void CreateSharedParameter(Application app, Document doc)
        {
            // Đường dẫn tới file Shared Parameter (có thể thay đổi)
            string sharedParameterFilePath = @"E:\RevitSharedParameters.txt";

            // Kiểm tra nếu file chưa tồn tại, tạo mới
            //if (!File.Exists(sharedParameterFilePath))
            //{
            //    File.Create(sharedParameterFilePath).Close();
            //}

            // Gán file Shared Parameter
            app.SharedParametersFilename = sharedParameterFilePath;
            DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();

            if (sharedParameterFile == null)
            {
                TaskDialog.Show("Error", "Không thể mở file Shared Parameter.");
            }

            RunTransaction(doc, "Create Schedule", (Transaction t) =>
            {
                //// Nhóm Shared Parameter
                //DefinitionGroup group = sharedParameterFile.Groups.get_Item("WallParameters") ?? sharedParameterFile.Groups.Create("WallParameters");

                //// Danh sách các parameter cần tạo
                //string[] paramNames = { "Wall_Width", "Wall_Height", "Wall_Length" };
                //ForgeTypeId[] paramTypes = { SpecTypeId.Length, SpecTypeId.Length, SpecTypeId.Length };
                string[] paramNames = { "W", "L", "H" };
                Category wallCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Walls);
                CategorySet categories = app.Create.NewCategorySet();
                categories.Insert(wallCategory);
                BindingMap bindingMap = doc.ParameterBindings;
                // Nhóm chứa các Shared Parameter trong file
                DefinitionGroup group = sharedParameterFile.Groups.get_Item("WallParameters");
                if (group == null)
                {
                    TaskDialog.Show("Error", "Không tìm thấy nhóm WallParameters trong file Shared Parameters.");
                }
                foreach (var paramName in paramNames)
                {
                    //if (group.Definitions.get_Item(paramName) == null)
                    //{
                    //    ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(paramName, paramTypes[Array.IndexOf(paramNames, paramName)])
                    //    {
                    //        Visible = true
                    //    };
                    //    Definition definition = group.Definitions.Create(options);

                    //    Binding binding = app.Create.NewInstanceBinding(categories);
                    //    BindingMap bindingMap = doc.ParameterBindings;
                    //    bindingMap.Insert(definition, binding, BuiltInParameterGroup.PG_GEOMETRY);
                    //}
                    Definition definition = group.Definitions.get_Item(paramName);
                    if (definition == null)
                    {
                        TaskDialog.Show("Error", "Không tìm thấy Shared Parameter" + paramName);
                    }

                    // Xóa nếu đã tồn tại (để tránh trùng lặp)
                    if (IsParameterAlreadyBound(doc, definition))
                    {
                        bindingMap.Remove(definition);
                    }
                    InstanceBinding binding = app.Create.NewInstanceBinding(categories);
                    //bindingMap.Insert(definition, binding, BuiltInParameterGroup.PG_GEOMETRY);
                    bindingMap.Insert(definition, binding, BuiltInParameterGroup.PG_DATA);
                }
            });
        }

        // Kiểm tra xem Shared Parameter đã được gán vào Project chưa.
        private bool IsParameterAlreadyBound(Document doc, Definition definition)
        {
            BindingMap bindingMap = doc.ParameterBindings;
            return bindingMap.Contains(definition);
        }

        private ElementId GetSharedParameterId(Document doc, string paramName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Wall wall = collector.OfClass(typeof(Wall)).FirstOrDefault() as Wall;

            foreach (Parameter param in wall.Parameters)
            {
                if (param.Definition.Name == paramName)
                {
                    return param.Id;
                }
            }
            return ElementId.InvalidElementId;
        }

        [Obsolete]
        private void CreateNewSchedule(Document doc)
        {
            RunTransaction(doc, "Create Schedule", (Transaction t) =>
            {
                // 1. Tạo Schedule mới cho Walls
                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Walls));
                // 2. Đặt tên cho Schedule
                schedule.Name = "My Wall Schedule";
                // 3. Lấy định nghĩa của Schedule
                ScheduleDefinition definition = schedule.Definition;

                ScheduleFieldId typeId = AddScheduleField(definition, BuiltInParameter.ELEM_TYPE_PARAM, false); // Loại tường
                ScheduleFieldId typeCommentsId = AddScheduleField(definition, BuiltInParameter.ALL_MODEL_TYPE_COMMENTS, true);
                ScheduleFieldId typeMarkId = AddScheduleField(definition, BuiltInParameter.ALL_MODEL_TYPE_MARK, true);
                ScheduleFieldId descriptionId = AddScheduleField(definition, BuiltInParameter.ALL_MODEL_DESCRIPTION, true);
                ScheduleFieldId baseContraintId = AddScheduleField(definition, BuiltInParameter.WALL_BASE_CONSTRAINT, false);
                ScheduleFieldId commentsId = AddScheduleField(definition, BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, false);
                ScheduleFieldId markId = AddScheduleField(definition, BuiltInParameter.ALL_MODEL_MARK, false);
                ScheduleFieldId unconnectedHeightId = AddScheduleField(definition, BuiltInParameter.WALL_USER_HEIGHT_PARAM, false);
                //ScheduleFieldId areaId = AddScheduleField(definition, BuiltInParameter.HOST_AREA_COMPUTED, "Diện Tích"); // Diện tích
                //ScheduleFieldId lengthId = AddScheduleField(definition, BuiltInParameter.CURVE_ELEM_LENGTH, "Chiều Dài"); // Chiều dài

                List<ElementId> paramIds = new List<ElementId>();

                foreach (string paramName in new string[] { "W", "L", "H" })
                {
                    ElementId paramId = GetSharedParameterId(doc, paramName);
                    if (paramId != ElementId.InvalidElementId)
                    {
                        ScheduleField field = definition.AddField(ScheduleFieldType.Instance, paramId);
                        paramIds.Add(field.ParameterId);
                    }
                }

                IList<TableCellCombinedParameterData> conbinedParams = new List<TableCellCombinedParameterData>();

                foreach (var id in paramIds)
                {
                    TableCellCombinedParameterData data = TableCellCombinedParameterData.Create();
                    data.ParamId = id;

                    //Thêm dấu phân cách("_") nếu chưa phải phần tử cuối
                    if (id != paramIds[paramIds.Count - 1])
                    {
                        data.Separator = "_";
                    }
                    conbinedParams.Add(data);
                }

                ScheduleField combinedField = definition.InsertCombinedParameterField(conbinedParams, "Kích thước = W_L_H", definition.GetFieldCount());

                //ScheduleField countField = definition.AddField(ScheduleFieldType.Count);
                //countField.ColumnHeading = "Số lượng";

                //6 Sắp xếp (Sorting)

                ScheduleSortGroupField sortLevelIdField = new ScheduleSortGroupField(baseContraintId, ScheduleSortOrder.Ascending) { ShowHeader = true };
                //ScheduleSortGroupField sortLevelIdField = new ScheduleSortGroupField(baseContraintId, ScheduleSortOrder.Ascending);

                definition.InsertSortGroupField(sortLevelIdField, 0);

                ScheduleSortGroupField sortTypeIdField = new ScheduleSortGroupField(typeId, ScheduleSortOrder.Ascending);
                definition.InsertSortGroupField(sortTypeIdField, 1);
                // 7 Lọc dữ liệu (Chỉ lấy cửa có chiều rộng > 800mm)
                //double lengthInFeet = 800 * 0.00328084;
                //ScheduleFilter lengthFilter = new ScheduleFilter(lengthId, ScheduleFilterType.GreaterThan, lengthInFeet);
                //definition.AddFilter(lengthFilter);

                // 8 Tính tổng số lượng tường
                definition.IsItemized = false;
                //definition.ShowGrandTotal = true; // Hiển thị tổng số lượng của mỗi nhóm

                //TableData tableData = schedule.GetTableData();
                //TableSectionData headerSection = tableData.GetSectionData(SectionType.Header);

                //TableMergedCell mergedCell = new TableMergedCell(0, 1, 0, 2);

                //headerSection.MergeCells(mergedCell);
                //headerSection.SetCellText(0, 1, "Kích thước");
            });
        }

        private ScheduleFieldId AddScheduleField(ScheduleDefinition definition, BuiltInParameter parameter, bool isTypeParameter)
        {
            ScheduleFieldType fieldType = isTypeParameter ? ScheduleFieldType.ElementType : ScheduleFieldType.Instance;
            ScheduleField field = definition.AddField(fieldType, new ElementId(parameter));

            //field.ColumnHeading = columnHeading;
            field.IsHidden = false;
            return field.FieldId;
        }

        //End________________
    }
}