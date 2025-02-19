using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class PlaceFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Find family
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FamilySymbol symbol = collector.OfClass(typeof(FamilySymbol)).WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .First(x => x.Name == "1525 x 762mm");

            //FamilySymbol symbol = null;

            //foreach (FamilySymbol sym in symbols)
            //{
            //    if (sym.Name == "1525 x 762mm")
            //    {
            //        symbol = sym as FamilySymbol;
            //        break;
            //    }
            //}

            try
            {
                using (Transaction trans = new Transaction(doc, "Place family"))
                {
                    trans.Start();
                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                    }
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
    }
}