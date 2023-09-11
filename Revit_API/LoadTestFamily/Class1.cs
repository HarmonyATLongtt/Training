using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using System.Diagnostics;
using System.Windows.Forms;

namespace LoadTestFamily
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            Family family = null;
            string familyPath = null;

            // Get all level in doc current
            FilteredElementCollector collectLevels
                = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level));

            // Get first level to use (draw)
            Level firstLevel = collectLevels.FirstElement() as Level;

            Selection sel = uiApp.ActiveUIDocument.Selection;

            // Create a transaction to use...
            using(var transaction = new Transaction(doc, "pick"))
            {
                transaction.Start();

                try
                {
                    // Get all family symbol have in current project
                    FilteredElementCollector testFamily = new FilteredElementCollector(doc);
                    testFamily.OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_GenericModel);

                    // Get the family has name if "Test"
                    FamilySymbol firstTest = testFamily.Cast<FamilySymbol>().FirstOrDefault(f => f.FamilyName.Equals("Test"));

                    // Check family is available in current project ?
                    if(firstTest is null)
                    {
                        // Declare path to family need load
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Filter = "Family (*.rfa)|*.rfa";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            familyPath = dialog.FileName;
                        }

                        // Load family follow family path
                        doc.LoadFamily(familyPath, out family);

                        // Get all family symbol in project
                        testFamily = new FilteredElementCollector(doc);
                        testFamily.OfClass(typeof(FamilySymbol))
                            .OfCategory(BuiltInCategory.OST_GenericModel);

                        // Get family name 'Test'
                        firstTest = testFamily.Cast<FamilySymbol>().FirstOrDefault(f => f.FamilyName.Equals("Test"));
                    }

                    // Choose two point to set location
                    XYZ p1 = sel.PickPoint("Please pick first point...");
                    XYZ p2 = sel.PickPoint("Please pick second point...");

                    // Calculate middle point, which is location of element
                    XYZ middlePoint = (p1 + p2) / 2;

                    // Check family symbol is active ?
                    if (!firstTest.IsActive)
                        firstTest.Activate();

                    // Create an instance of family 'Test' and assign it for elem variable
                    Element elem = doc.Create
                        .NewFamilyInstance(middlePoint, firstTest, firstLevel, Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming);

                    // Get parameter name 'Length'
                    Parameter p = elem.LookupParameter("Length");

                    // Calculate distance between point1 and point2 use Pytago
                    p.Set(Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2)));

                    // Reference to location point of element
                    LocationPoint lp = elem.Location as LocationPoint;

                    // Create axis to rotate
                    Line axis = Line.CreateBound(middlePoint, middlePoint + new XYZ(0, 0, 10));

                    // Calculate angle use ArcTan
                    lp.Rotate(axis, -lp.Rotation - Math.Atan((p2.X - p1.X) / (p2.Y - p1.Y)));
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }

                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}
