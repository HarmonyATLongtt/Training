using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Forms;
using System;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_InstanceInLevel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            XYZ location = uiDoc.Selection.PickPoint("Pick point to place column...");

            Form_InstanceInLevel f = new Form_InstanceInLevel(commandData);
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
                    if (!columnSymbol.IsActive)
                        columnSymbol.Activate();

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