using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ExRevitAPI
{
    [Transaction(TransactionMode.Manual)]
    class AddFileRevit : IExternalCommand
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
                libraryPath = "c:\\";   // If not have, use a default path.
            }

            // Allow the user to select a family file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = libraryPath;
            openFileDialog.Filter = "Family Files (*.rfa)|*.rfa";

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
                            String name = familyFilePath;
                            TaskDialog.Show("Family đã được tải thành công", "Family Name: " + name);
                            // Family đã được tải thành công, thực hiện các thao tác khác (nếu cần)
                        } 
                        else
                        {
                            TaskDialog.Show("Error", "Failed to load family file.");
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Error", "Failed to load family file: " + ex.Message);
                    }
                    transaction.Commit();

                }
            }

            return Result.Succeeded;
        }
    }
}
