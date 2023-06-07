using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace CreateFamily.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreateGridDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            IList<Reference> selectedGrids = uidoc.Selection.PickObjects(ObjectType.Element, "Please select Grid");
            XYZ selectedPoint = uidoc.Selection.PickPoint();

            ReferenceArray referenceArray = new ReferenceArray();

            foreach (Reference gridRef in selectedGrids)
            {
                referenceArray.Append(gridRef);
            }

            View activeView = uidoc.ActiveView;

            using (Transaction transaction = new Transaction(doc, "Create Dimension"))
            {
                transaction.Start();

                List<XYZ> points = new List<XYZ>();
                foreach (Reference gridRef in selectedGrids)
                {
                    Element gridElement = doc.GetElement(gridRef.ElementId);
                    if (gridElement is Grid grid)
                    {
                        Curve gridCurve = grid.Curve;
                        if (gridCurve is Line line)
                        {
                            Line lineUnbound = Line.CreateUnbound(line.Origin, line.Direction);
                            var intersectionResult = lineUnbound.Project(selectedPoint);
                            if (intersectionResult != null)
                                points.Add(intersectionResult.XYZPoint);
                        }
                    }
                }

                Line lineDim = Line.CreateBound(points[0], points[1]);

                doc.Create.NewDimension(activeView, lineDim, referenceArray);

                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}