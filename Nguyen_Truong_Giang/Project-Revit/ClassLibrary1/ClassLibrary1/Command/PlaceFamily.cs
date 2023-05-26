using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class PlaceFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            //find Family
            FamilySymbol symbol = new FilteredElementCollector(doc)
                                .WhereElementIsElementType()
                                .OfClass(typeof(FamilySymbol))
                                .Cast<FamilySymbol>()
                                .FirstOrDefault(x => x.Name == "Default");

            if (symbol != null)
            {
                try
                {
                    using (Transaction trans = new Transaction(doc, "Place Family"))
                    {
                        trans.Start();
                        if (!symbol.IsActive)
                            symbol.Activate();

                        doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        trans.Commit();
                    }
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return Result.Failed;
                }
            }
            else
            {
                TaskDialog.Show("symbol not found", "found");
            }

            return Result.Cancelled;
        }
    }
}