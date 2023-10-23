using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MultiRoom.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiRoom
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Pick multi-room
            var roomsSelection = uiDoc.Selection.PickObjects(ObjectType.Element, new FilterElementExtension(e => e is Room), "Select rooms...");

            // Add rooms to list
            List<Room> rooms = new List<Room>();
            foreach (var roomsItem in roomsSelection)
                rooms.Add(doc.GetElement(roomsItem) as Room);

            // Set option to get bound of room
            SpatialElementBoundaryOptions optios = new SpatialElementBoundaryOptions();
            optios.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;

            int index = 1;
            double totalArea = 0;
            using (var trans = new Transaction(doc))
            {
                #region Load Family and merge room

                trans.Start("Create rectangle...");
                // Load family
                Family family = new LoadFamily().GetFamily(doc);
                FamilySymbol symbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;

                // Create solid for room just picked
                IList<CurveLoop> profileLoops = new List<CurveLoop>();
                Solid roomsSolid = null;
                foreach (var roomItem in rooms)
                {
                    profileLoops.Clear();
                    CurveLoop curves = new CurveLoop();
                    var list = roomItem.GetBoundarySegments(optios)[0];
                    foreach (var seg in list)
                        curves.Append(seg.GetCurve() as Line);
                    profileLoops.Add(curves);
                    if (roomsSolid != null)
                        roomsSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomsSolid, CreateExtrusionGeometry(profileLoops), BooleanOperationsType.Union);
                    else
                        roomsSolid = CreateExtrusionGeometry(profileLoops);
                }

                #endregion Load Family and merge room

                while (roomsSolid.SurfaceArea > 0)
                {
                    // Get all edges is line bottom and top of solid
                    List<Line> lines = new List<Line>();
                    foreach (Edge i in roomsSolid.Edges)
                    {
                        var line = i.AsCurve() as Line;
                        if ((line.Direction.X == 1 || line.Direction.X == -1) && line.Origin.Z > 0)
                        {
                            lines.Add(line);
                        }
                    }

                    // Sort list lines by y min
                    var tempLines = lines.OrderBy(lin => lin.GetEndPoint(0).Y);

                    // Get line have y min
                    var firstLine = tempLines.First();

                    double firstX = Math.Round(firstLine.GetEndPoint(0).X, 5);
                    double secondX = Math.Round(firstLine.GetEndPoint(1).X, 5);

                    // Remove line was picked from list
                    lines.Remove(firstLine);
                    tempLines = lines.OrderBy(lin => lin.GetEndPoint(0).X);
                    // Get point follow direction left to right
                    if (firstX > secondX)
                    {
                        var temp = secondX;
                        secondX = firstX;
                        firstX = temp;
                    }

                    // Find area max
                    double yMin = double.MaxValue;
                    double yMax = 0, maxArea = 0, tempX = 0;
                    FamilyInstance instance = null;
                    foreach (Line line in tempLines)
                    {
                        double fx = Math.Round(line.GetEndPoint(0).X, 5);
                        double sx = Math.Round(line.GetEndPoint(1).X, 5);
                        if (fx > sx)
                        {
                            var temp = sx;
                            sx = fx;
                            fx = temp;
                        }
                        if (fx >= firstX && sx <= secondX ||
                            fx < secondX && sx >= secondX ||
                            fx <= firstX && sx > firstX ||
                            fx <= firstX && sx >= secondX)
                        {
                            if (yMin > Math.Round(line.GetEndPoint(0).Y, 5))
                                yMin = Math.Round(line.GetEndPoint(0).Y, 5);
                            if (!symbol.IsActive)
                                symbol.Activate();

                            if (sx >= secondX)
                                sx = secondX;

                            if (maxArea < (sx - firstX) * (yMin - firstLine.GetEndPoint(0).Y))
                            {
                                maxArea = (sx - firstX) * (yMin - firstLine.GetEndPoint(0).Y);
                                yMax = yMin;
                                tempX = sx;
                            }
                        }
                    }

                    XYZ location = new XYZ((tempX + firstX) / 2, (firstLine.GetEndPoint(0).Y + yMax) / 2, 0);

                    // Handle line have small length
                    if (Math.Round(tempX - firstX, 2) != 0)
                    {
                        instance = doc.Create.NewFamilyInstance(location, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                        instance.LookupParameter("エリア・幅").Set(tempX - firstX);
                        instance.LookupParameter("エリア・長さ").Set(yMax - firstLine.GetEndPoint(0).Y);

                        doc.Regenerate();

                        instance.LookupParameter("エリア名称").Set(index++.ToString());
                        totalArea += instance.LookupParameter("エリア・面積").AsDouble();
                    }

                    double z = 0;
                    XYZ p1 = new XYZ(firstX, yMax, z);
                    XYZ p2 = new XYZ(firstX, firstLine.GetEndPoint(0).Y, z);
                    XYZ p3 = new XYZ(tempX, firstLine.GetEndPoint(0).Y, z);
                    XYZ p4 = new XYZ(tempX, yMax, z);

                    CurveLoop curves = new CurveLoop();
                    bool check = false;

                    try
                    {
                        curves.Append(Line.CreateBound(p1, p2));
                        curves.Append(Line.CreateBound(p2, p3));
                        curves.Append(Line.CreateBound(p3, p4));
                        curves.Append(Line.CreateBound(p4, p1));
                    }
                    catch
                    {
                        // Handle not create sloid has small length
                        p1 = new XYZ(firstX, yMax, z);
                        p2 = new XYZ(firstX, firstLine.GetEndPoint(0).Y, z);
                        p3 = new XYZ(firstX + 0.01, firstLine.GetEndPoint(0).Y, z);
                        p4 = new XYZ(firstX + 0.01, yMax, z);

                        curves = new CurveLoop();
                        curves.Append(Line.CreateBound(p1, p2));
                        curves.Append(Line.CreateBound(p2, p3));
                        curves.Append(Line.CreateBound(p3, p4));
                        curves.Append(Line.CreateBound(p4, p1));
                        check = true;
                    }

                    profileLoops.Clear();
                    profileLoops.Add(curves);

                    var tempRoomSolid = CreateExtrusionGeometry(profileLoops);

                    if (!check)
                        roomsSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomsSolid, tempRoomSolid, BooleanOperationsType.Difference);
                    else
                    {
                        var sameSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomsSolid, tempRoomSolid, BooleanOperationsType.Intersect);
                        roomsSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomsSolid, sameSolid, BooleanOperationsType.Difference);
                    }
                }
                trans.Commit();
                totalArea = UnitUtils.ConvertFromInternalUnits(totalArea, DisplayUnitType.DUT_SQUARE_METERS);
                TaskDialog.Show("Message", "Total family instace was placed: " + (index - 1) +
                    "\nTotal area of family instace was placed: " + totalArea + "m" + (char)178);
            }

            return Result.Succeeded;
        }

        private Solid CreateExtrusionGeometry(IList<CurveLoop> profileLoops)
        {
            return GeometryCreationUtilities.CreateExtrusionGeometry(profileLoops, new XYZ(0, 0, 1), 10.0f);
        }
    }
}