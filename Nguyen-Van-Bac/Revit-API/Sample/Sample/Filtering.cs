using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Mechanical;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public class Filtering
    {
        public static void GetAllWalls(Document document)
        {
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            FilteredElementCollector collector = new FilteredElementCollector(document);
            IList<Element> walls = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements();
            String prompt = "The walls in the current document are:\n";
            foreach (Element e in walls)
            {
                prompt += e.Name + "\n";
            }
            TaskDialog.Show("Revit", prompt);
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateDoorInWall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Lấy tham chiếu đến tài liệu hiện tại
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            // Lấy danh sách các mức trong tài liệu
            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            // Tạo một danh sách tên các mức để hiển thị trong hộp thoại
            List<string> levelNames = levels.Select(x => x.Name).ToList();

            // Hiển thị hộp thoại chọn mức
            TaskDialogSelectLevel dialog = new TaskDialogSelectLevel("Chọn một mức");
            dialog.LevelNames = levelNames;
            //if (dialog.Show() != TaskDialogResult.Ok)
            //{
            //    return Result.Cancelled;
            //}

            // Lấy tên mức được chọn từ hộp thoại
            string selectedLevelName = "Level 1";

            // Lấy mức từ tên của nó
            Level selectedLevel = levels.FirstOrDefault(x => x.Name == selectedLevelName);

            //if (selectedLevel == null)
            //{
            //    TaskDialog.Show("Error", "Không tìm thấy mức được chọn.");
            //    return Result.Failed;
            //}

            // Tạo một tường mới
            using (Transaction tx = new Transaction(doc, "ok"))
            {
                tx.Start();
                //Wall wall = Wall.Create(doc, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)), selectedLevel.LevelId, false);
                Wall wall = new CreateWall().CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uiDoc, new XYZ(0, 0, 0), new XYZ(10, 0, 0));
                tx.Commit();
                // Lấy đoạn tường được tạo ra
                LocationCurve wallLocation = wall.Location as LocationCurve;
                Line wallLine = wallLocation.Curve as Line;

                // Tạo một điểm nằm giữa tường để đặt cửa
                XYZ midpoint = (wallLine.GetEndPoint(0) + wallLine.GetEndPoint(1)) / 2;

                // Tìm biểu tượng của cửa
                FamilySymbol doorSymbol = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .FirstOrDefault(x => x.FamilyName == "M_Curtain Wall Single");

                if (doorSymbol != null)
                {
                    // Tạo một đối tượng cửa với kích thước và vị trí cụ thể, và gắn nó vào tường
                    FamilyInstance doorInstance = doc.Create.NewFamilyInstance(midpoint, doorSymbol, wall, selectedLevel, StructuralType.NonStructural);

                    // Cài đặt các thuộc tính khác của cửa nếu cần
                    // Ví dụ: doorInstance.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("D01");
                }
            }

            return Result.Succeeded;
        }
    }

    public class TaskDialogSelectLevel : TaskDialog
    {
        public List<string> LevelNames { get; set; }
        public string SelectedLevelName { get; private set; }

        public TaskDialogSelectLevel(string instruction) : base(instruction)
        {
            SelectedLevelName = "";
            CommonButtons = TaskDialogCommonButtons.Cancel;
        }

        public TaskDialogResult Show()
        {
            foreach (string levelName in LevelNames)
            {
                AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + LevelNames.IndexOf(levelName), levelName);
            }

            TaskDialogResult result = base.Show();

            if (result == TaskDialogResult.CommandLink1 || result == TaskDialogResult.CommandLink2 || result == TaskDialogResult.CommandLink3)
            {
                SelectedLevelName = GetSelectedLevelName(result);
            }

            return result;
        }

        private string GetSelectedLevelName(TaskDialogResult result)
        {
            int index = (int)result - (int)TaskDialogCommandLinkId.CommandLink1;
            if (index >= 0 && index < LevelNames.Count)
            {
                return LevelNames[index];
            }
            return "";
        }
    }
}