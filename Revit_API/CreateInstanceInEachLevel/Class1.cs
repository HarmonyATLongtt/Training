using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace CreateInstanceInEachLevel
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            XYZ location = uiDoc.Selection.PickPoint("Pick point to place column...");

            Form1 f = new Form1(commandData);
            f.ShowDialog();


            FamilySymbol columnSymbol = new FilteredElementCollector(doc)
               .OfClass(typeof(FamilySymbol))
               .OfCategory(BuiltInCategory.OST_StructuralColumns)
               .Cast<FamilySymbol>()
               .First();

            Level level = f.Level;

            using (var trans = new Transaction(doc, "Create new instance in selected level"))
            {
                trans.Start();

                try
                {
                    if (doc.Create.NewFamilyInstance(location, columnSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column) != null)
                        TaskDialog.Show("Message", "Create Column Successful");
                    else
                        TaskDialog.Show("Message", "Create Column Fail");
                }
                catch (Exception ex)
                {
                    message = ex.ToString();
                    return Result.Failed;
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}