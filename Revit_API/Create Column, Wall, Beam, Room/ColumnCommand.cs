using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace CreateColumnApi
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ColumnCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            XYZ point = uiApp.ActiveUIDocument.Selection.PickPoint();

            FilteredElementCollector familySymbols
                = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .OfClass(typeof(FamilySymbol));

            FamilySymbol column = familySymbols.FirstElement() as FamilySymbol;

            Level levelUse = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Level))
                .FirstElement() as Level;


            using (var transaction = new Transaction(doc, "Create Column"))
            {
                transaction.Start();
                try
                {
                    if (!column.IsActive)
                        column.Activate();

                    try
                    {
                        doc.Create.NewFamilyInstance(point, column, levelUse, Autodesk.Revit.DB.Structure.StructuralType.Column);
                    }
                    catch
                    {

                    }
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
}
