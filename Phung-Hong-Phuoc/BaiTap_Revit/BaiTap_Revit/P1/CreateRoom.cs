using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace BaiTap_Revit
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateRoom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Prompt user to select points for the boundary
                IList<XYZ> selectedPoints = GetBoundaryPoints(uidoc);
                if (selectedPoints.Count < 3)
                {
                    message = "At least three points are required to create a boundary.";
                    return Result.Failed;
                }

                using (Transaction tx = new Transaction(doc, "Create Room"))
                {
                    tx.Start();

                    // Create a boundary for the room
                    CurveArray boundary = new CurveArray();
                    for (int i = 0; i < selectedPoints.Count; i++)
                    {
                        XYZ start = selectedPoints[i];
                        XYZ end = selectedPoints[(i + 1) % selectedPoints.Count];
                        boundary.Append(Line.CreateBound(start, end));
                    }

                    // Get the level for the room
                    Level level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .WhereElementIsNotElementType()
                        .FirstOrDefault() as Level;

                    if (level != null)
                    {
                        // Create walls to enclose the room
                        foreach (Curve curve in boundary)
                        {
                            Wall.Create(doc, curve, level.Id, false);
                        }

                        // Calculate the centroid of the boundary
                        XYZ roomLocation = CalculateCentroid(selectedPoints);

                        // Create the room at the calculated centroid
                        Room newRoom = doc.Create.NewRoom(level, new UV(roomLocation.X, roomLocation.Y));

                        // Set room properties if necessary
                        if (newRoom != null)
                        {
                            newRoom.Name = "New Room";
                        }
                    }
                    else
                    {
                        message = "Level not found.";
                        return Result.Failed;
                    }

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

        private IList<XYZ> GetBoundaryPoints(UIDocument uidoc)
        {
            // Implement user interaction to select points
            List<XYZ> points = new List<XYZ>();

            // Keep selecting points until the user finishes (press Esc or right-click)
            try
            {
                while (true)
                {
                    XYZ point = uidoc.Selection.PickPoint("Select boundary point (Press Esc to finish)");
                    points.Add(point);
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User finished selecting points
            }

            return points;
        }

        private XYZ CalculateCentroid(IList<XYZ> points)
        {
            double x = points.Average(p => p.X);
            double y = points.Average(p => p.Y);
            double z = points.Average(p => p.Z);
            return new XYZ(x, y, z);
        }
    }
}