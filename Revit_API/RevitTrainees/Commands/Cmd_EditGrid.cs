using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Forms;
using RevitTrainees.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_EditGrid : IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Pick two grid
            Reference r1 = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new FilterElementExtension(e => e is Grid), "Select first grid");
            Reference r2 = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new FilterElementExtension(e => e is Grid), "Select second grid");

            // Create references array
            ReferenceArray references = new ReferenceArray();
            references.Append(r1);
            references.Append(r2);

            // Get element from reference
            Element e1 = doc.GetElement(r1);
            Element e2 = doc.GetElement(r2);

            HashSet<int> elems = new HashSet<int>
            {
                e1.Id.IntegerValue,
                e2.Id.IntegerValue
            };

            // Get origin of grid
            XYZ p1 = ((e1 as Grid).Curve as Line).Origin;
            XYZ originOfP2 = ((e2 as Grid).Curve as Line).Origin;
            XYZ p2 = null;
            UV uv = new UV();
            double distance = 0;

            // Calculate point 2
            Plane planeReference = Plane.CreateByNormalAndOrigin(((e1 as Grid).Curve as Line).Direction, p1);
            planeReference.Project(originOfP2, out uv, out distance);

            // Get angle to get reference point
            double angle = planeReference.Normal.AngleTo(originOfP2 - p1);

            if (angle > Math.PI / 2)
            {
                p2 = planeReference.Normal * distance + originOfP2;
            }
            else
            {
                p2 = originOfP2 - planeReference.Normal * distance;
            }

            // Create line to get length of it
            Line line = Line.CreateBound(p1, p2);

            // Get origin distance between two grid
            double originDis = line.Length;

            // Get direction of p1, p2
            XYZ direction = p2 - p1;
            // Get direction normalize
            direction = direction.Normalize();

            try
            {
                using (var trans = new Transaction(doc, "Create dimension"))
                {
                    trans.Start();

                    Dimension dim = doc.Create.NewDimension(doc.ActiveView, line, references);
                    if (dim.Value != -1)
                    {
                        TaskDialog.Show("Distance between two grid", "Distance is: " + UnitUtils.ConvertFromInternalUnits(line.Length, DisplayUnitType.DUT_MILLIMETERS).ToString());
                    }
                    else
                    {
                        message = "Two grid just select is not parallel!";
                        return Result.Failed;
                    }

                    // Create form get input from user
                    Form_DistanceOfGrid f = new Form_DistanceOfGrid();
                    f.ShowDialog();

                    // Get new distance from user
                    double newDis = UnitUtils.ConvertToInternalUnits(f.distance, DisplayUnitType.DUT_MILLIMETERS);

                    // Calculate t is coefficient of direction vector to move
                    double t = 0;
                    if (f.check)
                    {
                        t = newDis - originDis;
                    }

                    Dimension dimm = null;

                    bool check = false;

                    foreach (var elem in e2.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_Constraints)))
                    {
                        if (doc.GetElement(elem) is Dimension d)
                        {
                            var di = d.References;

                            var item1 = di.get_Item(0).ElementId.IntegerValue;
                            var item2 = di.get_Item(1).ElementId.IntegerValue;

                            check = elems.Contains<int>(item1) && elems.Contains<int>(item2);

                            if (check)
                            {
                                dimm = d;
                                break;
                            }
                        }
                    }

                    if (dimm != null)
                        dimm.IsLocked = false;

                    ElementTransformUtils.MoveElement(doc, e2.Id, t * direction);

                    if (dimm != null)
                        dimm.IsLocked = true;

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