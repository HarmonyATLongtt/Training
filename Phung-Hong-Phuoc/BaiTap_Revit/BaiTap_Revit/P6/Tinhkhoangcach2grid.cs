using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BaiTap_Revit.P6
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Tinhkhoangcach2grid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Select the grids
                Reference ref1 = uidoc.Selection.PickObject(ObjectType.Element, "Select the first grid");
                Reference ref2 = uidoc.Selection.PickObject(ObjectType.Element, "Select the second grid");

                Autodesk.Revit.DB.Grid grid1 = doc.GetElement(ref1) as Autodesk.Revit.DB.Grid;
                Autodesk.Revit.DB.Grid grid2 = doc.GetElement(ref2) as Autodesk.Revit.DB.Grid;

                if (grid1 == null || grid2 == null)
                {
                    TaskDialog.Show("Error", "One or both selected elements are not grids.");
                    return Result.Failed;
                }

                using (Transaction tx = new Transaction(doc, "Adjust Grid Distance"))
                {
                    tx.Start();

                    // Calculate the distance between the two grids
                    double distance = CalculateDistanceBetweenGrids(grid1, grid2);
                    TaskDialog.Show("Grid Distance", $"Current distance between grids: {distance} mm");

                    // Pick a new point for the second grid
                    XYZ newPoint = uidoc.Selection.PickPoint("Pick a new location for the second grid");

                    // Adjust the position of the second grid
                    AdjustGridPosition(doc, grid2, newPoint);

                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        // Adjusts the position of the grid to the new point
        private void AdjustGridPosition(Document doc, Autodesk.Revit.DB.Grid grid, XYZ newPoint)
        {
            Line gridLine = grid.Curve as Line;

            if (gridLine != null)
            {
                XYZ currentEndPoint = gridLine.GetEndPoint(0);
                XYZ moveVector = newPoint - currentEndPoint;

                // Move the grid element
                ElementTransformUtils.MoveElement(doc, grid.Id, moveVector);
            }
        }

        // Calculates the distance between two parallel grids
        public double CalculateDistanceBetweenGrids(Autodesk.Revit.DB.Grid grid1, Autodesk.Revit.DB.Grid grid2)
        {
            Curve curve1 = grid1.Curve;
            Curve curve2 = grid2.Curve;

            // Get the direction vectors of both grids
            XYZ dir1 = (curve1.GetEndPoint(1) - curve1.GetEndPoint(0)).Normalize();
            XYZ dir2 = (curve2.GetEndPoint(1) - curve2.GetEndPoint(0)).Normalize();

            // Check if the grids are parallel
            if (!dir1.CrossProduct(dir2).IsAlmostEqualTo(XYZ.Zero))
            {
                TaskDialog.Show("Error", "The selected grids are not parallel.");
                throw new InvalidOperationException("Grids are not parallel.");
            }

            // Get a point on grid1 and find the closest point on grid2
            XYZ pointOnCurve1 = curve1.GetEndPoint(0);
            XYZ closestPointOnCurve2 = curve2.Project(pointOnCurve1).XYZPoint;
            double distance = pointOnCurve1.DistanceTo(closestPointOnCurve2);

            // Convert distance from feet to millimeters
            double distanceInMillimeters = distance * 304.8;
            return distanceInMillimeters;
        }
    }
}