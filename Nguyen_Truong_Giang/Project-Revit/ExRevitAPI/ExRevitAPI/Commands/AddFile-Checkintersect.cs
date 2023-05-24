using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ExRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class AddFile_Checkintersect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            ImportRevitFile(doc);

            return Result.Succeeded;
        }

        private List<XYZ> GetCreatePoint(Document doc)
        {
            TaskDialog dialog = new TaskDialog("Lựa chọn đặt family");
            dialog.MainInstruction = "Chọn lựa chọn đặt family";
            dialog.MainContent = "lựa chọn đặt family lên 1 giao điểm hoặc tất cả giao điểm đã tìm thấy.";

            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Đặt 1 giao điểm");

            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Đặt tất cả giao điểm");

            //lay toan bo grid trong project
            List<Grid> grids = getAllGrid(doc);

            //tim giao diem cua 2 grid
            Dictionary<string, XYZ> dic = new Dictionary<string, XYZ>();
            for (int i = 0; i < grids.Count; i++)
            {
                for (int j = i + 1; j < grids.Count; j++)
                {
                    XYZ p = GetIntersection(grids[i], grids[j]);
                    if (p != null)
                        dic.Add(grids[i].Name + "-" + grids[j].Name, p);
                }
            }
            if (dic.Count == 1 && dic.Count <= 1)
                return dic.Values.ToList();
            else if (dic.Count > 1)
            {
                TaskDialogResult result = dialog.Show();
                if (result == TaskDialogResult.CommandLink1) //chọn "Đặt 1 giao điểm"
                    return new List<XYZ> { SelectMultiIntersect(doc, dic) };
                else if (result == TaskDialogResult.CommandLink2) //chọn "Đặt tất cả giao điểm"
                    return dic.Values.ToList();
                else
                    return null;
            }
            else
            {
                TaskDialog.Show("Lỗi", "Không tìm thấy giao điểm");
                return null;
            }
        }

        private List<Grid> getAllGrid(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                    .OfCategory(BuiltInCategory.OST_Grids)
                                                    .OfClass(typeof(Grid))
                                                    .Cast<Grid>()
                                                    .ToList();
        }

        private XYZ GetIntersection(Grid g1, Grid g2)
        {
            if (g1 != null && g2 != null)
            {
                var ret = g1.Curve.Intersect(g2.Curve, out IntersectionResultArray result);
                if (ret == SetComparisonResult.Overlap && result.Size == 1)
                {
                    return result.get_Item(0).XYZPoint;
                }
            }
            return null;
        }

        private void ImportRevitFile(Document doc)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Family Files (*.rfa)|*.rfa";
            openFileDialog.Title = "Chọn file Revit để import";

            // Hiển thị hộp thoại chọn file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                using (Transaction trans = new Transaction(doc, "import File to REVIT"))
                {
                    trans.Start();
                    Family family = null;
                    bool load = doc.LoadFamily(filePath, out family);
                    if (load && family != null)
                    {
                        FamilySymbol familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                        if (!familySymbol.IsActive)
                        {
                            familySymbol.Activate();
                        }
                        if (familySymbol != null)
                        {
                            foreach (XYZ point in GetCreatePoint(doc))
                            {
                                if (point != null)
                                {
                                    doc.Create.NewFamilyInstance(point, familySymbol, SetLevel(doc), Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                                    TaskDialog.Show("Family đã được tải thành công", familySymbol.FamilyName + " đã được tải thành công");
                                }
                            }

                            //doc.Create.NewFamilyInstance(GetCreatePoint(doc), familySymbol, SetLevel(doc), Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            trans.Commit();
                        }
                    }

                    if (load == false)
                    {
                        TaskDialog.Show("Lỗi", "File Family đã tồn tại trong Revit");
                        return;
                    }

                    if (!File.Exists(filePath))
                    {
                        TaskDialog.Show("Lỗi", "Đường dẫn file không hợp lệ");
                        return;
                    }
                }
            }
        }

        public Level SetLevel(Document doc)
        {
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            Level level = levelCollector
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>().First(x => x.Name == "Level 1");

            return level;
        }

        public List<XYZ> PickMultiIntersect(Document doc)
        {
            UIDocument uidoc = new UIDocument(doc);
            Selection choices = uidoc.Selection;

            Grid selectedGrid = null;
            IList<Reference> gridRefs = null;

            try
            {
                gridRefs = choices.PickObjects(ObjectType.Element, new GridFilterSelection());
            }
            catch (System.Exception)
            {
            }

            if (gridRefs != null && gridRefs.Count > 1)
            {
                foreach (var refer in gridRefs)
                {
                    Grid grid = doc.GetElement(refer) as Grid;
                    selectedGrid = grid;
                }
            }

            return null;
        }

        public XYZ SelectMultiIntersect(Document doc, Dictionary<string, XYZ> points)
        {
            UIDocument uidoc = new UIDocument(doc);
            Selection choices = uidoc.Selection;

            XYZ selectedPoint = null;

            // danh sách các tùy chọn
            List<string> options = new List<string>();

            foreach (var dic in points)
            {
                string option = $"Giao điểm: " + dic.Key;
                options.Add(option);
            }

            // Hiển thị dialog
            TaskDialog dialog = new TaskDialog("Chọn giao điểm");
            dialog.MainInstruction = "Chọn giao điểm để đặt family";
            dialog.MainContent = "Các giao điểm có sẵn:";
            dialog.CommonButtons = TaskDialogCommonButtons.Cancel;

            // Thêm tùy chọn vào dialog
            foreach (string option in options)
            {
                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + options.IndexOf(option), option);
            }

            // Hiển thị dialog và xử lý lựa chọn
            TaskDialogResult result = dialog.Show();

            if (result != TaskDialogResult.Cancel)
            {
                int selectedIndex = (int)result - (int)TaskDialogCommandLinkId.CommandLink1;
                if (selectedIndex >= 0 && selectedIndex < points.Count)
                {
                    selectedPoint = points.Values.ToList()[selectedIndex];
                }
            }
            if (result == TaskDialogResult.Cancel)
            {
                return null;
            }

            return selectedPoint;
        }
    }
}