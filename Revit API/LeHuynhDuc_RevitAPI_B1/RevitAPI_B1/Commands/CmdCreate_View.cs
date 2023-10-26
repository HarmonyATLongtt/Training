using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreate_View : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;


            TaskDialog taskDialog = new TaskDialog("Create view");

            // Thiết lập nội dung và thông điệp của TaskDialog
            taskDialog.MainInstruction = "Create_View";
            taskDialog.MainContent = "Select option:";

            // Thêm các nút lựa chọn
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Create Structural Plan");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Create Floor Plan");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Create Ceiling plane");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Create 3D view");

            // Thiết lập nút mặc định (optional)
            taskDialog.DefaultButton = TaskDialogResult.CommandLink1;

            // Hiển thị TaskDialog và lấy kết quả lựa chọn
            TaskDialogResult result = taskDialog.Show();

            // Xử lý kết quả lựa chọn
            if (result == TaskDialogResult.CommandLink1)
            {
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                                            .OfClass(typeof(ViewFamilyType))
                                            .Cast<ViewFamilyType>()
                                            .First(x => x.ViewFamily == ViewFamily.StructuralPlan);

                Level level = new FilteredElementCollector(doc)
                                .OfClass(typeof(Level))
                                .OfCategory(BuiltInCategory.OST_Levels)
                                .WhereElementIsNotElementType()
                                .Cast<Level>()
                                .First(x => x.Name == "Level 1");
                using (Transaction trans = new Transaction(doc, "create_StructuralPlan"))
                {
                    trans.Start();
                    ViewPlan viewPlan = ViewPlan.Create(doc, viewFamilyType.Id, level.Id);
                    //viewPlan.Name = "New level";
                    trans.Commit();
                }
            }
            else if (result == TaskDialogResult.CommandLink2)
            {
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                                            .OfClass(typeof(ViewFamilyType))
                                            .Cast<ViewFamilyType>()
                                            .First(x => x.ViewFamily == ViewFamily.FloorPlan);

                Level level = new FilteredElementCollector(doc)
                                  .OfClass(typeof(Level))
                                  .OfCategory(BuiltInCategory.OST_Levels)
                                  .WhereElementIsNotElementType()
                                  .Cast<Level>()
                                  //.First(x => x.Name == "Level 1");
                                  .Last();
                using (Transaction trans = new Transaction(doc, "create_FloorPlane"))
                {
                    trans.Start();
                    ViewPlan viewPlan = ViewPlan.Create(doc, viewFamilyType.Id, level.Id);
                    //viewPlan.Name = "New level";
                    trans.Commit();
                }
            }
            else if (result == TaskDialogResult.CommandLink3)
            {
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                                             .OfClass(typeof(ViewFamilyType))
                                             .Cast<ViewFamilyType>()
                                             .First(x => x.ViewFamily == ViewFamily.CeilingPlan);

                Level level = new FilteredElementCollector(doc)
                                .OfClass(typeof(Level))
                                .OfCategory(BuiltInCategory.OST_Levels)
                                .WhereElementIsNotElementType()
                                .Cast<Level>()
                                .First(x => x.Name == "Level 1");
                using (Transaction trans = new Transaction(doc, "create_CeilingPlane"))
                {
                    trans.Start();
                    ViewPlan viewPlan = ViewPlan.Create(doc, viewFamilyType.Id, level.Id);
                    //viewPlan.Name = "New level";
                    trans.Commit();
                }
            }
            else if (result == TaskDialogResult.CommandLink4)
            {
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                                             .OfClass(typeof(ViewFamilyType))
                                             .Cast<ViewFamilyType>()
                                             .First(x => x.ViewFamily == ViewFamily.ThreeDimensional);

                Level level = new FilteredElementCollector(doc)
                                .OfClass(typeof(Level))
                                .OfCategory(BuiltInCategory.OST_Levels)
                                .WhereElementIsNotElementType()
                                .Cast<Level>()
                                .First(x => x.Name == "Level 1");
                using (Transaction trans = new Transaction(doc, "create_3DPlane"))
                {
                    trans.Start();
                    View3D view3D = View3D.CreateIsometric(doc, viewFamilyType.Id);
                    //viewPlan.Name = "New level";
                    trans.Commit();
                }
            }
            return Result.Succeeded;
        }
    }
}
