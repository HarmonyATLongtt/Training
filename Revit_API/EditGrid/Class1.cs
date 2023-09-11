using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace EditGrid
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Pick two grid
            Reference r1 = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new FilterElementExtension(e => e is Grid), "Select first grid");
            Reference r2 = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new FilterElementExtension(e => e is Grid), "Select second grid");

            // Get element from reference
            Element e1 = doc.GetElement(r1);
            Element e2 = doc.GetElement(r2);

            // Get origin of grid
            XYZ p1 = ((e1 as Grid).Curve as Line).Origin;
            XYZ p2 = ((e2 as Grid).Curve as Line).Origin;

            // Create line to get length of it
            Line line = Line.CreateBound(p1, p2);
            TaskDialog.Show("Distance between two grid", "Distance is: " + UnitUtils.ConvertFromInternalUnits(line.Length, DisplayUnitType.DUT_MILLIMETERS).ToString());

            // Get origin distance between two grid
            double originDis = line.Length;

            // Get direction of p1, p2
            XYZ direction = p2 - p1;
            // Get direction normalize
            direction = direction.Normalize();

            // Create form get input from user
            Form1 f = new Form1();
            f.ShowDialog();

            // Get new distance from user
            double newDis = UnitUtils.ConvertToInternalUnits(f.distance, DisplayUnitType.DUT_MILLIMETERS);

            double t;

            // Check if new distance < origin distance then vector is minus
            if (originDis > newDis)
            {
                t = -newDis + originDis;
            }
            else
                t = newDis - originDis;

            try
            {
                using (var trans = new Transaction(doc, "Create dimension"))
                {
                    trans.Start();

                    ElementTransformUtils.MoveElement(doc, e2.Id, t * direction);

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
