using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;

namespace BaiTap_Revit.P3
{
    [Transaction(TransactionMode.Manual)]
    internal class GanEntity : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Tạo Schema (nếu chưa tồn tại)
                Guid schemaId = new Guid("5BAE2664-C7E8-459D-B73A-3D8801800297");
                Schema schema = Schema.Lookup(schemaId);
                if (schema == null)
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(schemaId);
                    schemaBuilder.SetSchemaName("MyCustomSchema");

                    // Định nghĩa trường CustomString
                    schemaBuilder.AddSimpleField("CustomString", typeof(string));
                    schemaBuilder.AddSimpleField("NewNumber", typeof(int));
                    schemaBuilder.AddSimpleField("NewName", typeof(string));

                    schema = schemaBuilder.Finish();
                }

                // Chọn nhiều đối tượng
                IList<Reference> selectedObjects = uidoc.Selection.PickObjects(ObjectType.Element, "Chọn các đối tượng để gắn entity");
                if (selectedObjects == null || selectedObjects.Count == 0)
                {
                    TaskDialog.Show("Warming", "Không có đối tượng nào được chọn.");
                    return Result.Failed;
                }

                using (Transaction tx = new Transaction(doc, "Thiết lập Entity cho nhiều đối tượng"))
                {
                    tx.Start();

                    // Lặp qua từng đối tượng được chọn và gắn entity
                    foreach (Reference obj in selectedObjects)
                    {
                        Element element = doc.GetElement(obj.ElementId);

                        // Kiểm tra xem entity có tồn tại trên element hay không
                        Entity entity = element.GetEntity(schema);
                        if (!entity.IsValid()) // Nếu chưa tồn tại, tạo mới
                        {
                            entity = new Entity(schema);
                        }

                        // Gán giá trị cho các trường
                        entity.Set("CustomString", "Giá trị tùy chỉnh của tôi");
                        entity.Set("NewNumber", 15);
                        entity.Set("NewName", "My new schema");

                        // Gán entity cho element
                        element.SetEntity(entity);
                    }

                    tx.Commit();
                }

                TaskDialog.Show("Success", "Entity đã được gắn thành công vào các đối tượng được chọn.");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = $"Có lỗi xảy ra: {ex.Message}";
                return Result.Failed;
            }
        }
    }
}