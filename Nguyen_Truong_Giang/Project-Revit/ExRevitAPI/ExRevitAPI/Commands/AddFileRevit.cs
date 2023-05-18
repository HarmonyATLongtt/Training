using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
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
                                    var t = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                                    string name = familyFilePath;
                                    TaskDialog.Show("Family đã được tải thành công", name + " đã được tải thành công");
                                }
                            }
                            else
                            {
                                TaskDialog.Show("Error", "Failed to load family file.");
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Error", ex.Message);
                        }
                        transaction.Commit();
                    }
                }
            }
            return Result.Succeeded;
        }
    }
}