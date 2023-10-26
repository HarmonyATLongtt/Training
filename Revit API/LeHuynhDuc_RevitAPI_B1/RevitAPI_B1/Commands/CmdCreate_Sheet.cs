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
    public class CmdCreate_Sheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            FamilySymbol symbol = new FilteredElementCollector(doc)
                                .OfClass(typeof(FamilySymbol))
                                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                .Cast<FamilySymbol>()
                                .First();
            using (Transaction trans = new Transaction(doc, "Create Sheet"))
            {
                trans.Start();
                ViewSheet sheet1 = ViewSheet.Create(doc, symbol.Id);
                sheet1.Name = "New sheet";
                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
}
