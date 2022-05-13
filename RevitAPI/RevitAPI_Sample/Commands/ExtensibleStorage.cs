using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using RevitAPI_Sample.DataClasses;
using RevitAPI_Sample.Utils;
using RevitAPI_Sample.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RevitBinding = Autodesk.Revit.DB.Binding;

#nullable enable

namespace RevitAPI_Sample.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateExtensibleStorage : IExternalCommand
    {
        private Document _doc;
        private Guid _guid = new Guid("{92A9548D-764F-40C9-B46D-30B6FC2B0363}");
        private string infor = Define.PARA_INFO;
        private string index = Define.PARA_INDEX;
        private string material = Define.PARA_MATERIAL;
        private string height = Define.PARA_HEIGHT;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            _doc = uidoc.Document;

            //Category cate = Category.GetCategory(_doc, BuiltInCategory.OST_StructuralFraming);
            //if (!IsAllParameterExist(cate, Define.GetAllParaNames))
            //{
            //    System.Windows.Forms.MessageBox.Show("Parameter not enough or have not created yet");
            //    return Result.Cancelled;
            //}
            IList<Element> frammingElems = Utils.Common.PickElements(uidoc, new FrammingElementFilter(), "Select structural Frammings");
            if (frammingElems.Count == 0)
            {
                return Result.Cancelled;
            }
            Schema schema = Schema.Lookup(_guid);
            if (schema == null || !schema.WriteAccessGranted())
            {
                return Result.Cancelled;
            }
            using (Transaction tr = new Transaction(_doc))
            {
                foreach (var ele in frammingElems)
                {
                    if (ele.GetEntity(schema) is Entity entity && entity.IsValid())
                    {
                        tr.Start("Update value");
                        UpdateField(ele, entity);

                        tr.Commit();
                    }
                    else
                    {
                        tr.Start("create storage");
                        CreateExtensibleStorageElement(ele);

                        tr.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }

        private bool IsAllParameterExist(Category category, string[] paramateNames)
        {
            var ids = TableView.GetAvailableParameters(_doc, category.Id);
            string s = string.Empty;
            IEnumerable<ElementId>? finded = ids.Where(x => paramateNames.Contains(GetParameterName(x)));
            return paramateNames.Length == finded.Count();
        }

        private string GetParameterName(ElementId parameterId)
        {
            string paramName = String.Empty;
            if (Enum.IsDefined(typeof(BuiltInParameter), parameterId.IntegerValue))
            {
                paramName = LabelUtils.GetLabelFor((BuiltInParameter)parameterId.IntegerValue);
            }
            else
            {
                if (_doc.GetElement(parameterId) is ParameterElement parameterElement)
                {
                    Definition definition = parameterElement.GetDefinition();

                    paramName = definition is not null ? definition.Name : parameterId.IntegerValue.ToString();
                }
            }

            return paramName;
        }

        private Entity? CreateExtensibleStorageElement(Element elem)
        {
            Entity? entity = null;
            if (elem == null)
            {
                return entity;
            }

            SchemaBuilder schemaBuilder = new SchemaBuilder(_guid);
            schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
            schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
            schemaBuilder.SetVendorId("DataFramingId");
            schemaBuilder.SetSchemaName("DataFraming");

            FieldBuilder builderInfor = schemaBuilder.AddSimpleField(infor, typeof(string));
            builderInfor.SetDocumentation("for infor framing");
            FieldBuilder builderIndex = schemaBuilder.AddSimpleField(index, typeof(int));
            builderIndex.SetDocumentation("for index framing");
            FieldBuilder builderMaterial = schemaBuilder.AddSimpleField(material, typeof(int));
            builderMaterial.SetDocumentation("for infor framing material");
            FieldBuilder builderHeight = schemaBuilder.AddSimpleField(height, typeof(double));
            builderHeight.SetUnitType(UnitType.UT_Length);
            builderHeight.SetDocumentation("for infor framing height");
            // register the Schema object
            Schema schema = schemaBuilder.Finish();

            // create an entity (object) for this schema (class)

            // get the field from the schema

            entity = new Entity(schema);
            elem.SetEntity(entity);// store the entity in the element

            return entity;
        }

        private void UpdateField(Element ele, Entity entity)
        {
            Parameter paraInfor = ele.LookupParameter(infor);
            if (paraInfor != null && paraInfor.HasValue)
            {
                entity.Set(infor, paraInfor.AsString());
            }
            Parameter paraIndex = ele.LookupParameter(index);
            if (paraIndex != null && paraIndex.HasValue)
            {
                entity.Set(index, paraIndex.AsInteger());
            }
            Element eleType = ele.Document.GetElement(ele.GetTypeId());
            Parameter paraMaterial = eleType.LookupParameter(material);
            if (paraMaterial != null && paraMaterial.HasValue)
            {
                entity.Set(material, paraMaterial.AsInteger());
            }
            Parameter paraHeight = eleType.LookupParameter(height);
            if (paraHeight != null && paraHeight.HasValue)
            {
                entity.Set(height, paraHeight.AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
            }
            ele.SetEntity(entity);
        }

        private Type? GetValueField<Type>(Element elem, Field field)
        {
            Type? result = default;

            Schema schema = field.Schema;
            Entity entity = elem.GetEntity(schema);
            if (entity.IsValid())
            {
                result = entity.Get<Type>(field);
            }
            return result;
        }
    }
}