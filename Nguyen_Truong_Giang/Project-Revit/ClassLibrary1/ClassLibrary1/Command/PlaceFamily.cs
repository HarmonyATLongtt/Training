using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    class PlaceFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            //find Family
            FilteredElementCollector collecter = new FilteredElementCollector(doc);
            FamilySymbol symbol = collecter.OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>().FirstOrDefault(x => x.Name == "Default");

            //FamilySymbol symbol = null;

            //foreach (FamilySymbol sym in symbols)
            //{
            //    if(sym.Name == "Default")
            //    {
            //        symbol = sym as FamilySymbol;
            //        break;
            //    }
            //}

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
            else {
                TaskDialog.Show("symbol not found","found");
            }


            return Result.Cancelled;
        }
    }

}
