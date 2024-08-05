using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreatNewSchema : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Tạo các trường thông tin cho schema
                Guid Id = Guid.NewGuid();
                SchemaBuilder schemaBuilder = new SchemaBuilder(Id);
                schemaBuilder.SetSchemaName("NewSchema");
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);

                // Thêm các trường vào schema
                schemaBuilder.AddSimpleField("NewLength", typeof(double)).SetSpec(SpecTypeId.Length);
                schemaBuilder.AddSimpleField("NewNumber", typeof(int));
                schemaBuilder.AddSimpleField("NewName", typeof(string));

                // Tạo schema
                Schema schema = schemaBuilder.Finish();

                // Tạo entity từ schema
                Entity entity = new Entity(schema);
                entity.Set("NewLength", 100.2, UnitTypeId.Meters);
                entity.Set("NewNumber", 12);
                entity.Set("NewName", "Creat Schema");

                using (Transaction trans = new Transaction(doc, "Set schema to element"))
                {
                    trans.Start();
                    // Lấy phần tử và gán entity vào nó
                    Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                    if (reference != null)
                    {
                        Element element = doc.GetElement(reference);
                        element.SetEntity(entity);
                    }
                    trans.Commit();
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