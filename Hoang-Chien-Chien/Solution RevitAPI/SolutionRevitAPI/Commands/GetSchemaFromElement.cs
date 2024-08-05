using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class GetSchemaFromElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            try
            {
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                if (reference != null)
                {
                    Element element = doc.GetElement(reference);
                    IList<Guid> entities = element.GetEntitySchemaGuids();
                    string ans = string.Empty;
                    foreach (Guid entityGuid in entities)
                    {
                        Schema schema = Schema.Lookup(entityGuid);
                        ans += $"{schema.SchemaName}\n";
                        // Lấy entity từ phần tử
                        Entity entity = element.GetEntity(schema);
                        IList<Field> fields = schema.ListFields();
                        if (entity.IsValid())
                        {
                            foreach (Field field in fields)
                            {
                                string fieldName = field.FieldName;
                                Type fieldType = field.ValueType;
                                if (fieldType == typeof(string))
                                {
                                    string result = entity.Get<string>(fieldName);
                                    ans += $"  {fieldName}: {result}\n";
                                }
                                if (fieldType == typeof(double))
                                {
                                    ForgeTypeId specTypeId = field.GetSpecTypeId();
                                    double result;
                                    if (specTypeId == SpecTypeId.Length)
                                    {
                                        result = entity.Get<double>(fieldName, UnitTypeId.Meters);
                                    }
                                    else
                                        result = entity.Get<double>(fieldName);
                                    ans += $"\t  {fieldName}: {result} m\n";
                                }
                                if (fieldType == typeof(int))
                                {
                                    int result = entity.Get<int>(fieldName);
                                    ans += $"  {fieldName}: {result}\n";
                                }
                            }
                        }
                    }
                    TaskDialog.Show("Schema Infor", ans);
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng hủy bỏ việc chọn đối tượng
                return Result.Cancelled; ;
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