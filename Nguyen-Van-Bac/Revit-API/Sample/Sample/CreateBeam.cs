using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateBeam : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;

            try
            {
                Transaction tx = new Transaction(doc, "Create Beam");
                tx.Start();
                CreateBeamInstance(doc, doc.ActiveView.GenLevel, uiDoc);
                tx.Commit();
            }
            catch
            {
                message = "Unexpected Exception thrown.";
                return Result.Failed;
            }
            TaskDialog.Show("OK", "Đã thêm dầm");
            return Result.Succeeded;
        }

        public FamilyInstance CreateBeamInstance(Document doc, Level lv, UIDocument uiDoc)
        {
            // get the given view's level for beam creation
            Level level = doc.GetElement(lv.Id) as Level;

            // get a family symbol
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFraming);
            FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;
            if (!gotSymbol.IsActive)
            {
                gotSymbol.Activate();
            }
            // create new beam 10' long starting at origin
            XYZ startPoint = uiDoc.Selection.PickPoint();
            XYZ endPoint = uiDoc.Selection.PickPoint();
            Curve beamLine = Line.CreateBound(startPoint, endPoint);

            // create a new beam
            return doc.Create.NewFamilyInstance(beamLine, gotSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
        }
    }
}