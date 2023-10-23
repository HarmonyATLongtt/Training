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

            SpatialElementBoundaryOptions optios = new SpatialElementBoundaryOptions();
            optios.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;

            int index = 1;
            double totalArea = 0;

            using (var trans = new Transaction(doc))
            {
                #region Load Family and merge room

                trans.Start("Load family...");
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

                trans.Commit();

                #endregion Load Family and merge room

                while (Math.Round(roomsSolid.SurfaceArea, 5) > 0)
                {
                    #region Create family instance

                    trans.Start("Create family instace number " + index);
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

                    // Get point follow direction left to right
                    if (firstX > secondX)
                    {
                        var temp = secondX;
                        secondX = firstX;
                        firstX = temp;
                    }

                    // Find y min, where have line intersect
                    double yMin = double.MaxValue;
                    foreach (Line line in lines)
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
                        }
                    }

                    // Create family instance and set parameter for it
                    if (!symbol.IsActive)
                        symbol.Activate();
                    XYZ location = new XYZ((secondX + firstX) / 2, (firstLine.GetEndPoint(0).Y + yMin) / 2, 0);
                    var instance = doc.Create.NewFamilyInstance(location, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    instance.LookupParameter("エリア・幅").Set(secondX - firstX);
                    instance.LookupParameter("エリア・長さ").Set(yMin - firstLine.GetEndPoint(0).Y);
                    instance.LookupParameter("エリア名称").Set(index++.ToString());

                    trans.Commit();

                    #endregion Create family instance

                    // Calculate total area
                    totalArea += instance.LookupParameter("エリア・面積").AsDouble();

                    #region Create new room from instace was placed and calculate area not set

                    trans.Start("Execute some task...");

                    double z = 0;
                    XYZ p1 = new XYZ(firstX, yMin, z);
                    XYZ p2 = new XYZ(firstX, firstLine.GetEndPoint(0).Y, z);
                    XYZ p3 = new XYZ(secondX, firstLine.GetEndPoint(0).Y, z);
                    XYZ p4 = new XYZ(secondX, yMin, z);

                    CurveLoop curves = new CurveLoop();
                    curves.Append(Line.CreateBound(p1, p2));
                    curves.Append(Line.CreateBound(p2, p3));
                    curves.Append(Line.CreateBound(p3, p4));
                    curves.Append(Line.CreateBound(p4, p1));

                    profileLoops.Clear();
                    profileLoops.Add(curves);

                    var tempRoomSolid = CreateExtrusionGeometry(profileLoops);

                    roomsSolid = BooleanOperationsUtils.ExecuteBooleanOperation(roomsSolid, tempRoomSolid, BooleanOperationsType.Difference);
                    trans.Commit();

                    #endregion Create new room from instace was placed and calculate area not set
                }
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