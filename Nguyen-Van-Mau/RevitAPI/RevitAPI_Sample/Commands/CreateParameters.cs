using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAPI_Sample.DataClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RevitBinding = Autodesk.Revit.DB.Binding;

#nullable enable

namespace RevitAPI_Sample.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateShareParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            ExternalDefinitionCreationOptions infoOpt = new ExternalDefinitionCreationOptions(Define.PARA_INFO, ParameterType.Text)
            {
                Visible = true,
                Description = "Description for",
                //HideWhenNoValue = true,
                UserModifiable = true,
            };
            ExternalDefinitionCreationOptions indexOpt = new ExternalDefinitionCreationOptions(Define.PARA_INDEX, ParameterType.Integer)
            {
                Visible = true,
                Description = "Description for",
                //HideWhenNoValue = true,
                UserModifiable = true,
            };
            ExternalDefinitionCreationOptions materialOpt = new ExternalDefinitionCreationOptions(Define.PARA_MATERIAL, ParameterType.Integer)
            {
                Visible = true,
                Description = "Description for",
                //HideWhenNoValue = true,
                UserModifiable = true,
            };
            ExternalDefinitionCreationOptions heightOpt = new ExternalDefinitionCreationOptions(Define.PARA_HEIGHT, ParameterType.Number)
            {
                Visible = true,
                Description = "Description for",
                //HideWhenNoValue = true,
                UserModifiable = true,
            };
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Create Share");
                CreateProjectShareParameter(doc, "Group1", infoOpt, new[] { BuiltInCategory.OST_StructuralFraming }, BuiltInParameterGroup.INVALID, true, true);
                CreateProjectShareParameter(doc, "Group1", indexOpt, new[] { BuiltInCategory.OST_StructuralFraming }, BuiltInParameterGroup.INVALID, isParaInstance: true);
                CreateProjectShareParameter(doc, "Group2", materialOpt, new[] { BuiltInCategory.OST_StructuralFraming }, BuiltInParameterGroup.INVALID);
                CreateProjectShareParameter(doc, "Group2", heightOpt, new[] { BuiltInCategory.OST_StructuralFraming, BuiltInCategory.OST_Walls }, BuiltInParameterGroup.INVALID);

                tr.Commit();
            }

            return Result.Succeeded;
        }

        private void CreateProjectShareParameter(Document doc, string groupName,
            ExternalDefinitionCreationOptions paraOption, BuiltInCategory[] catesApply,
            BuiltInParameterGroup paraGroupType, bool isParaInstance = false, bool allowParaVaryBetweenGroups = false)
        {
            //Get share parameter file
            string fileShare = doc.Application.SharedParametersFilename;
            //Create new fileShare
            string tempFileShare = Path.GetTempFileName() + ".txt";
            using (File.Create(tempFileShare))
            {
                doc.Application.SharedParametersFilename = tempFileShare;
            }
            DefinitionFile defFile = doc.Application.OpenSharedParameterFile();
            //Create share group
            DefinitionGroup group = defFile.Groups.get_Item(groupName);
            if (group == null)
            {
                group = defFile.Groups.Create(groupName);
            }
            //Create share parameter  ExternalDefinitionCreationOptions
            Definition def = group.Definitions.get_Item(paraOption.Name);
            if (def == null)
            {
                def = group.Definitions.Create(paraOption);
            }

            //Apply share para to category
            CategorySet catSet = doc.Application.Create.NewCategorySet();//  new CategorySet();
            foreach (var cate in catesApply)
            {
                catSet.Insert(Category.GetCategory(doc, cate));
            }
            RevitBinding bin = isParaInstance ? doc.Application.Create.NewInstanceBinding(catSet) : doc.Application.Create.NewTypeBinding(catSet);

            //apply parameter binding
            if (doc.ParameterBindings.Contains(def))
            {
                doc.ParameterBindings.ReInsert(def, bin, paraGroupType);
            }
            else
            {
                doc.ParameterBindings.Insert(def, bin, paraGroupType);
            }

            //SetAllowVaryBetweenGroups
            SetParaAllowVaryBetweenGroups(doc, paraOption.GUID, allowParaVaryBetweenGroups);
            //Apply old fileShare parameter
            using (File.Create(tempFileShare))
            {
                doc.Application.SharedParametersFilename = fileShare;
            }
            //Delete temp share parameter
            File.Delete(tempFileShare);
        }

        /// <summary>
        /// Finds the shared parameter element that corresponds to the given Guid.
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="guid">Shared parameter Guid</param>
        /// <returns>Set AllowVaryBetweenGroups if can</returns>
        private ICollection<ElementId> SetParaAllowVaryBetweenGroups(Document doc, Guid guid, bool allowVaryBetweenGroups = true)
        {
            ICollection<ElementId> result = new List<ElementId>();
            try
            {
                SharedParameterElement paramElem = SharedParameterElement.Lookup(doc, guid);
                if (paramElem != null && paramElem.GetDefinition().VariesAcrossGroups != allowVaryBetweenGroups)
                    result = paramElem.GetDefinition().SetAllowVaryBetweenGroups(doc, allowVaryBetweenGroups);
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Not run
        /// </summary>
        /// <param name="doc"></param>
        private void CreateProjectParameter(Document doc)
        {
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start();

                CategorySet catSet = new CategorySet();

                catSet.Insert(Category.GetCategory(doc, BuiltInCategory.OST_Doors));

                RevitBinding bin = doc.Application.Create.NewInstanceBinding(catSet);//: doc.Application.Create.NewTypeBinding(catSet);

                CategorySet catSet1 = doc.Application.Create.NewCategorySet();
                tr.Commit();
            }
        }
    }
}