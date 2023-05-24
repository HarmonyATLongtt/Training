using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ExRevitAPI
{
    [Transaction(TransactionMode.Manual)]
    internal class AddFileRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //lấy đối tượng Grid
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> gridElements = collector.OfClass(typeof(Grid)).ToElements();

            string libraryPath = "";
            app.GetLibraryPaths().TryGetValue("Imperial Library", out libraryPath);

            if (string.IsNullOrEmpty(libraryPath))
            {
                libraryPath = "c:\\";
            }

            // Allow the user to select a family file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = libraryPath;
            openFileDialog.Filter = "Family Files (*.rfa)|*.rfa";

            // level filtered Element Collector
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            Level level = levelCollector
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>().First(x => x.Name == "Level 1");
            if (level != null)
            {
                // Load the family file using LoadFamily method and then give information.
                if (DialogResult.OK == openFileDialog.ShowDialog())
                {
                    //foreach (XYZ point in intersectionPoints)
                    //{
                    //    taskDialog.AddCommandLink(point.ToString(), "Giao điểm: " + point.ToString());
                    //}

                    string familyFilePath = openFileDialog.FileName;

                    using (Transaction transaction = new Transaction(doc, "Load Family"))
                    {
                        transaction.Start();
                        try
                        {
                            Family family = null;
                            // Tạo đối tượng FamilySymbol từ tệp family
                            bool load = doc.LoadFamily(familyFilePath, out family);
                            if (load && family != null)
                            {
                                FamilySymbol familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                                if (!familySymbol.IsActive)
                                {
                                    familySymbol.Activate();
                                }

                                // Family đã được tải thành công
                                if (familySymbol != null)
                                {
                                    try
                                    {
                                        for (int i = 0; i < gridElements.Count - 1; i++)
                                        {
                                            Grid grid1 = gridElements[i] as Grid;
                                            for (int j = i + 1; j < gridElements.Count; j++)
                                            {
                                                Grid grid2 = gridElements[j] as Grid;

                                                Line line1 = grid1.Curve as Line;
                                                Line line2 = grid2.Curve as Line;

                                                if (line1 != null && line2 != null)
                                                {
                                                    // Tìm giao điểm giữa hai đoạn thẳng
                                                    IntersectionResultArray intersectionResults;
                                                    SetComparisonResult result = line1.Intersect(line2, out intersectionResults);

                                                    if (result == SetComparisonResult.Overlap && intersectionResults.Size > 0)
                                                    {
                                                        // Có giao điểm
                                                        foreach (IntersectionResult intersectionResult in intersectionResults)
                                                        {
                                                            List<XYZ> intersectionPoints = new List<XYZ>();
                                                            XYZ intersectionPoint = intersectionResult.XYZPoint;
                                                            intersectionPoints.Add(intersectionPoint);

                                                            // Tạo TaskDialog
                                                            TaskDialog taskDialog = new TaskDialog("Danh sách giao điểm");
                                                            taskDialog.MainInstruction = "chọn giao điểm";
                                                            taskDialog.MainContent = "Đặt Family vào giao điểm: " + intersectionPoint;
                                                            taskDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
                                                            taskDialog.DefaultButton = TaskDialogResult.Ok;

                                                            if (taskDialog.Show() == TaskDialogResult.Ok)
                                                            {
                                                                var t = doc.Create.NewFamilyInstance(intersectionPoint, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                                                                string name = familyFilePath;
                                                                TaskDialog.Show("Family đã được tải thành công", name + " đã được tải thành công");
                                                                transaction.Commit();
                                                            }
                                                            else
                                                            {
                                                                transaction.RollBack();
                                                                TaskDialog.Show("Family chưa được tải thành công", "Cancel");
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        TaskDialog.Show("Error", "Failed to load family file.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        TaskDialog.Show("Error", ex.Message);
                                        throw ex;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message);
                            throw ex;
                        }
                    }
                }
            }

            return Result.Succeeded;
        }

        private void doJob()
        {
            //timf giao diem

            //load family

            //dat instance cua family vao tat ca cac giao diem
        }

    }
}