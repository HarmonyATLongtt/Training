using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace CreateColumnApi
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BeamsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            Selection selection = uiApp.ActiveUIDocument.Selection;

            XYZ startPoint = selection.PickPoint("Pick start point...");
            XYZ endPoint = selection.PickPoint("Pick end point...");

            Level level = doc.ActiveView.GenLevel;

            FilteredElementCollector beamSymbols
                = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralFraming);

            FamilySymbol beam = beamSymbols.Cast<FamilySymbol>().First();


            using (var transaction = new Transaction(doc, "Create Beams"))
            {
                transaction.Start();

                if (!beam.IsActive)
                    beam.Activate();
                try
                {
                    doc.Create.NewFamilyInstance(Line.CreateBound(startPoint, endPoint), beam, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                }
                catch
                {

                }

                transaction.Commit();
            }


            return Result.Succeeded;
        }
    }
}
