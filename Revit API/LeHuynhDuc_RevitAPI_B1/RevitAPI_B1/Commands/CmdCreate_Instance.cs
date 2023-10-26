using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreate_Instance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FamilySymbol symbol = collector.OfClass(typeof(FamilySymbol))
                                    .WhereElementIsElementType()
                                    .Cast<FamilySymbol>()
                                    .FirstOrDefault(x => x.Name == "1 x 1");
            try
            {
                if (symbol != null)
                {
                    using (Transaction trans = new Transaction(doc, "Family"))
                    {
                        trans.Start();
                        if (!symbol.IsActive)
                        {
                            symbol.Activate();
                        }
                        doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        trans.Commit();
                    }
                }
                else
                    TaskDialog.Show("Thong bao", "loi!!");

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
