using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    internal class LoadFileRevitFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);

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
                            return Result.Failed;
                        }
                    }
                }
            }
            return Result.Succeeded;
        }
    }
}