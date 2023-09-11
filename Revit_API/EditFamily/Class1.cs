using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using System.Windows.Forms;

namespace EditFamily
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Family family = null;

            string familyPath = null;

            // Filter elements collector have type is Family
            FilteredElementCollector familyTest
                = new FilteredElementCollector(doc)
                .OfClass(typeof(Family));

            // Get family has name 'Test'
            family = familyTest.Cast<Family>().FirstOrDefault(f => f.Name.Equals("Test"));

            // Check if 'Test' is null (not found)
            if (family == null)
            {
                // Use open file dialog to get path of family need to load
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    familyPath = dialog.FileName;
                }

                using (var transaction = new Transaction(doc, "Load Family"))
                {
                    transaction.Start();
                    doc.LoadFamily(familyPath, out family);
                    transaction.Commit();
                }

            }
            // Get document of family to edit
            Document familyDoc = doc.EditFamily(family);

            // Get all element is instance of extrusion
            Element elem = new FilteredElementCollector(familyDoc)
            .WhereElementIsNotElementType().Cast<Element>().FirstOrDefault(x => x is Extrusion);

            using (var transaction = new Transaction(familyDoc, "Edit Family"))
            {
                transaction.Start();

                // Use transform util to copy element has elem.Id to origin location translate new xyz
                ElementTransformUtils.CopyElement(familyDoc, elem.Id, new XYZ(5, 0, 0));

                transaction.Commit();
            }
            familyDoc.Save();

            return Result.Succeeded;
        }
    }
}
