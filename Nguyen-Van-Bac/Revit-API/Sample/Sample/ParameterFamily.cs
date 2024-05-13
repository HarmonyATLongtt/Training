using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Document = Autodesk.Revit.DB.Document;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class ParameterFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                Family family = null;
                UIDocument uidoc = new UIDocument(doc);
                Application app = doc.Application;
                UIApplication uiapp = new UIApplication(app);

                // Mở tài liệu family và kích hoạt
                UIDocument famUIDoc = uiapp.OpenAndActivateDocument("D:\\Family2.rfa");
                Document doc2 = famUIDoc.Document;
                // Bắt đầu giao dịch để chỉnh sửa
                using (Transaction transaction = new Transaction(famUIDoc.Document, "Edit parameter family"))
                {
                    transaction.Start();

                    FamilyManager familyMgr = famUIDoc.Document.FamilyManager;
                    FamilyParameter param = familyMgr.get_Parameter("Width");

                    if (param == null)
                    {
                        // Thêm tham số nếu nó không tồn tại
                        param = famUIDoc.Document.FamilyManager.AddParameter("text", GroupTypeId.IdentityData, SpecTypeId.String.Text, false);
                    }

                    // Thiết lập giá trị cho tham số
                    familyMgr.Set(param, 2);

                    // Kết thúc giao dịch
                    transaction.Commit();
                    TaskDialog.Show("OK", " Set paramater Succeeded");
                }
                SaveAsOptions opt = new SaveAsOptions
                {
                    OverwriteExistingFile = true,
                };

                doc2.SaveAs(doc2.PathName, opt);
                if (!string.IsNullOrEmpty(doc.PathName))
                {
                    uiapp.OpenAndActivateDocument(
                      doc.PathName);

                    doc2.Close(false); // no problem here, says Remy
                    LoadFamily(doc, uiDoc, app);
                }
                else
                {
                    // Avoid using OpenAndActivateDocument

                    uidoc.RefreshActiveView();

                    //doc2.Close( false ); // Remy says: Revit throws the exception and doesn't close the file
                }
                TaskDialog.Show("OK", " Succeeded");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", message + "\n" + ex.StackTrace);
            }
            return Result.Succeeded;
        }

        public void CreateFamilyDoument(Application application)
        {
            string templateFileName = @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\English\Metric Generic Model.rft";
            Document familyDocument = application.NewFamilyDocument(templateFileName);

            if (familyDocument == null)
            {
                throw new Exception("Cannot open family document");
            }
        }

        public void CreateParameter(Autodesk.Revit.DB.Document doc, string parameterName, ParameterType parameterType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_StructuralColumns)
                        .WhereElementIsElementType();
            Family family = GetFamilyByName(doc, "Family2");
            if (family != null)
            {
                using (Transaction transaction = new Transaction(doc, "new Parameter Family"))
                {
                    transaction.Start();
                    FamilyManager familyManager = doc.FamilyManager;
                    familyManager.AddParameter(parameterName, GroupTypeId.Text, SpecTypeId.Custom, true);
                }
            }
        }

        private Family GetFamilyByName(Autodesk.Revit.DB.Document doc, string familyName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Family));
            Family family = collector.FirstOrDefault(x => x.Name == familyName) as Family;
            return family;
        }

        private void LoadFamily(Document doc, UIDocument uidoc, Application app)
        {
            string libraryPath = "";
            app.GetLibraryPaths().TryGetValue("Imperial Library", out libraryPath);
            if (string.IsNullOrEmpty(libraryPath))
            {
                libraryPath = "c:\\";
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = libraryPath;
            openFileDialog.Filter = "Family Files (*.rfa)|*.rfa";

            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            Level level = levelCollector.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().Cast<Level>().First(x => x.Name == "Level 1");
            if (level != null)
            {
                if (DialogResult.OK == openFileDialog.ShowDialog())
                {
                    string filePathFamily = openFileDialog.FileName;
                    using (Transaction transaction = new Transaction(doc, "Load Family"))
                    {
                        transaction.Start();
                        try
                        {
                            Family family = null;
                            bool load = doc.LoadFamily(filePathFamily, out family);
                            if (load && family != null)
                            {
                                FamilySymbol familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                                if (!familySymbol.IsActive)
                                {
                                    familySymbol.Activate();
                                    if (familySymbol != null)
                                    {
                                        XYZ point = uidoc.Selection.PickPoint();
                                        TaskDialog taskDialog = new TaskDialog("Xác nhận điểm được chọn");
                                        taskDialog.MainContent = "Đặt Family vào vị trí điểm: " + point;
                                        taskDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;
                                        if (taskDialog.Show() == TaskDialogResult.Ok)
                                        {
                                            doc.Create.NewFamilyInstance(point, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                                            string name = filePathFamily;
                                            TaskDialog.Show("Ok", name + " đã được tải thành công");
                                            transaction.Commit();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TaskDialog.Show("Error", "Load family không thành công");
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message);
                        }
                    }
                }
            }
        }
    }
}