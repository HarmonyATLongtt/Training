using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Utils;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var column = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_Columns)
                        .Cast<FamilySymbol>()
                        .First();

            XYZ point = new XYZ(0, 0, 0);

            try
            {
                using (var trans = new Transaction(doc, "Create new column"))
                {
                    trans.Start();

                    if (!column.IsActive)
                        column.Activate();

                    var colInf = doc.Create.NewFamilyInstance(point, column, doc.ActiveView.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                    new SomeUtils().GetInfor(colInf);
                    new SomeUtils().SetComments(colInf, "Some comment was set in here...");

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}