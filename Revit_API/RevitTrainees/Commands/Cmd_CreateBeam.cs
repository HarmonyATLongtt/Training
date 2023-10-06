using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Utils;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateBeam : IExternalCommand
    {
        [System.Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var beam = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                .Cast<FamilySymbol>()
                .First();

            XYZ startPoint = new XYZ(UnitUtils.ConvertToInternalUnits(-10, DisplayUnitType.DUT_METERS), 0, 0);
            XYZ endPoint = new XYZ(UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_METERS), 0, 0);

            Line line = Line.CreateBound(startPoint, endPoint);

            try
            {
                if (doc.ActiveView.GenLevel.Name.Equals("Level 1"))
                {
                    TaskDialog.Show("Warning", "Please chooes level higher level 1...");
                    return Result.Failed;
                }

                using (var trans = new Transaction(doc, "Create new beam"))
                {
                    trans.Start();

                    if (!beam.IsActive)
                    {
                        beam.Activate();
                    }
                    var beamInf = doc.Create.NewFamilyInstance(line, beam, doc.ActiveView.GenLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    new SomeUtils().GetInfor(beamInf);
                    new SomeUtils().SetComments(beamInf, "Some comment was set in here...");

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}