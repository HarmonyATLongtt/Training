using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace BaiTap_Revit
{
    [Transaction(TransactionMode.Manual)]
    internal class CreateBeam : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            // Define beam start and end points
            XYZ start = uidoc.Selection.PickPoint("Please pick the start point of the wall");
            XYZ end = uidoc.Selection.PickPoint("Please pick the end point of the wall");

            try
            {
                using (Transaction trans = new Transaction(doc, "Create Beam"))
                {
                    trans.Start();

                    // Define the beam line
                    Line beamLine = Line.CreateBound(start, end);

                    // Get a beam family symbol (assuming it's the first one)
                    FamilySymbol beamType = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_StructuralFraming)
                        .FirstOrDefault() as FamilySymbol;

                    // get the given view's level for beam creation
                    Level level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .FirstElement() as Level;

                    // get a family symbol
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming);

                    FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;

                    if (beamType != null)
                    {
                        // Make sure the beam type is activated
                        if (!beamType.IsActive)
                        {
                            beamType.Activate();
                            doc.Regenerate();
                        }

                        // Create the beam
                        FamilyInstance instance = doc.Create.NewFamilyInstance(beamLine, gotSymbol,
                                                                 level, StructuralType.Beam);
                    }
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}