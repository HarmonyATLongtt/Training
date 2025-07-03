using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Converters;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using FirstCommand.Support.Constants;
using FirstCommand.Support.DebugTest;
using FirstCommand.Support.DrawOnRevit;
using FirstCommand.Support.FaceHandle;
using FirstCommand.Support.GenericClass;
using FirstCommand.Support.GenericClass.ComparerClass;
using FirstCommand.Support.GeometryHandle;
using FirstCommand.Support.LineHandle;
using FirstCommand.Support.PlaneHandle;
using FirstCommand.Support.PointHandle;
using FirstCommand.Support.SolidHandle;
using FirstCommand.Support.TrianglesHandle;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GetAxisFromLetterCommand : IExternalCommand
    {
        private Transform transform = null;
        private bool isRevitLink = false;
        private List<RectangularDimensions> ListRectangularDimension = new List<RectangularDimensions>();
        private List<CylinderDimensions> ListCylinderDimension = new List<CylinderDimensions>();
        private Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Document doc = uidoc.Document;
            doc = uidoc.Document;

            try
            {
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.LinkedElement);
                if (r != null)
                {
                    Element linkInstance = uidoc.Document.GetElement(r.ElementId);
                    RevitLinkInstance rli = linkInstance as RevitLinkInstance;

                    Document linkDoc = rli.GetLinkDocument();

                    ElementId linkedElemId = r.LinkedElementId;

                    Element linkedElem = linkDoc.GetElement(linkedElemId);
                    transform = rli.GetTransform();

                    Options options = new Options();
                    //
                    options.IncludeNonVisibleObjects = true;
                    //
                    options.DetailLevel = ViewDetailLevel.Fine;
                    options.ComputeReferences = true;
                    GeometryElement elementGeo = linkedElem.get_Geometry(options);

                    List<PlanarFace> planarFaces = new List<PlanarFace>();
                    List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();
                    Dictionary<Solid, List<PlanarFace>> solidPlanarFaces = new Dictionary<Solid, List<PlanarFace>>();

                    var solid = CreateSolidFromMeshes(linkedElem);
                    //var tupleValue = GetGroupedFacesFromSolid(doc, solid);
                    //planarFaces = tupleValue.Item1;
                    //cylindricalFaces = tupleValue.Item2;
                    //solidPlanarFaces = tupleValue.Item3;

                    //foreach (GeometryObject geometryObj in elementGeo)
                    //{
                    //    if (geometryObj is Solid solid)
                    //    {
                    //        var tupleValue = GetGroupedFacesFromSolid(doc, solid);
                    //        planarFaces = tupleValue.Item1;
                    //        cylindricalFaces = tupleValue.Item2;
                    //        solidPlanarFaces = tupleValue.Item3;
                    //    }
                    //    else if (geometryObj is GeometryInstance geomInstance)
                    //    {
                    //        GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    //        foreach (GeometryObject geometryObject in instanceGeometry)
                    //        {
                    //            if (geometryObject is Solid nestedSolid)
                    //            {
                    //                //solids.Add(nestedSolid);
                    //                var tupleValue = GetGroupedFacesFromSolid(doc, nestedSolid);

                    //                planarFaces = tupleValue.Item1;
                    //                cylindricalFaces = tupleValue.Item2;
                    //                solidPlanarFaces = tupleValue.Item3;
                    //                //if (solidPlanarFaces.Count > 0)
                    //                //{
                    //                //    TestForDebug.ShowValues(SolidUtility.GetPointOnSolid(nestedSolid));
                    //                //}
                    //            }
                    //        }
                    //    }
                    //}

                    //if (cylindricalFaces.Count > 0)
                    //{
                    //    // với mesh thì không cần
                    //    var listCylindricalFaces = GetCylindricalFaces(cylindricalFaces, planarFaces);

                    //    var lineAndIntersectPointOnFaces = FindLinesAndIntersectionsOnFace(doc, planarFaces, listCylindricalFaces);

                    //    ListCylinderDimension = PreparePointsForDrawingModelLine(doc, lineAndIntersectPointOnFaces);
                    //}
                    //else
                    //{
                    //    PrepareSolidCuttingData(doc, solidPlanarFaces);
                    //}

                    var x = ListRectangularDimension;
                    var y = ListCylinderDimension;
                    return Result.Succeeded;
                }
                return Result.Failed;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng nhấn ESC
                TaskDialog.Show("Notif", "Command has been cancel by user.");
                return Result.Cancelled;
            }
        }

        //__Hình trụ//
        //Start_______

        private Solid CreateSolidFromMeshes(Element element)
        {
            List<Solid> solids = GeometryUtility.GetSolids(element, doc).Solids;
            //Lấy ra danh sách các triangles của element
            List<MeshTriangle> meshTriangles = new List<MeshTriangle>();
            foreach (var s in solids)
            {
                meshTriangles.AddRange(TrianglesUtility.ExtractTrianglesFromSolid(s));
            }
            bool isCylinder = false;
            bool isRectangle = false;
            IsElementCylinderOrRectangle(meshTriangles, ref isCylinder, ref isRectangle);

            if (isRectangle)
            {
                //Gom nhóm các triangel theo mặt phẳng
                var result = TrianglesUtility.GroupTrianglesByVertexAndNormal(meshTriangles, 2);

                var pointsOfTrianglesGroupComplex = new List<List<(XYZ, XYZ)>>();
                var pointsOfTrianglesGroupSimple = new List<List<(XYZ, XYZ)>>();

                foreach (var triangles in result)
                {
                    var dict = new Dictionary<UnorderedXYZPair, List<MeshTriangle>>();
                    foreach (var tri in triangles)
                    {
                        var XYZPairs = TrianglesUtility.GetXYZPairsOfTriangle(tri);
                        foreach (var pair in XYZPairs)
                        {
                            var newPair = new UnorderedXYZPair(pair.Item1, pair.Item2);
                            if (!dict.TryGetValue(newPair, out var list))
                            {
                                list = new List<MeshTriangle>();
                                dict[newPair] = list;
                            }

                            list.Add(tri);
                        }
                    }
                    // Danh sách các cạnh bên ngoài cùng
                    var pairs = new List<UnorderedXYZPair>();
                    foreach (var keyValue in dict)
                    {
                        if (keyValue.Value.Count == 1)
                        {
                            pairs.Add(keyValue.Key);
                        }
                    }

                    var groups = PointUtility.GroupColinearPairs(pairs);
                    //HashSet<XYZ> setPoints = new HashSet<XYZ>(new XYZComparer());
                    List<(XYZ, XYZ)> startEndPairs = new List<(XYZ, XYZ)>();
                    foreach (var group in groups)
                    {
                        var (startPoint, endPoint) = PointUtility.FindFurthestPointsInGroup(group);
                        startEndPairs.Add((startPoint, endPoint));

                        // dùng startPoint và endPoint làm đầu – cuối đoạn thẳng đại diện
                    }
                    if (startEndPairs.Count == 4)
                    {
                        pointsOfTrianglesGroupSimple.Add(startEndPairs.ToList());
                    }
                    else if (startEndPairs.Count > 4)
                    {
                        pointsOfTrianglesGroupComplex.Add(startEndPairs.ToList());
                        //TestForDebug.ShowPointsToDraw(setPoints.ToList());
                    }
                }

                if (pointsOfTrianglesGroupSimple.Count > 1 && pointsOfTrianglesGroupComplex.Count == 0)
                {
                    var group1 = pointsOfTrianglesGroupSimple[0];
                    var group1Points = new HashSet<XYZ>(group1.SelectMany(pair => new[] { pair.Item1, pair.Item2 }), new XYZComparer());

                    var group2 = pointsOfTrianglesGroupSimple
                        .Skip(1)
                        .FirstOrDefault(group => !group.Any(pair => group1Points.Contains(pair.Item1) || group1Points.Contains(pair.Item2)));
                    if (group2 != default)
                    {
                        return MakeAxisHeigthForExtrusion(group1, group2);
                    }
                }
                else if (pointsOfTrianglesGroupComplex.Count == 2)
                {
                    var group1 = pointsOfTrianglesGroupComplex[0];
                    var group2 = pointsOfTrianglesGroupComplex[1];
                    return MakeAxisHeigthForExtrusion(group1, group2);
                }
                return null;
            }
            else if (isCylinder)
            {
                var groupTrianglesAndAxis = new List<(XYZ, List<MeshTriangle>)>();

                var meshTrianglesMakePlarnarFace = TrianglesUtility.GroupTrianglesByVertexAndNormal(meshTriangles, 3).SelectMany(tri => tri).ToList();
                var setmeshTrianglesMakePlarnarFace = meshTrianglesMakePlarnarFace.ToHashSet();
                meshTriangles.RemoveAll(tr => setmeshTrianglesMakePlarnarFace.Contains(tr));

                bool stop = false;
                do
                {
                    GetCylindricalFaces(meshTriangles, groupTrianglesAndAxis, ref stop);
                }
                while (stop == false);

                //TaskDialog.Show("Noti", groupTrianglesAndAxis.Count.ToString());

                if (groupTrianglesAndAxis.Count > 0)
                {
                    var lines = new List<Line>();
                    foreach (var (axis, triangles) in groupTrianglesAndAxis)
                    {
                        //if (GeometryUtility.IsParallel(axis, XYZ.BasisZ, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE)) continue;
                        //TrianglesUtility.DrawTriangles(triangles, doc, isRevitLink, transform);
                        var points = TrianglesUtility.GetVerticesOfAllTriangles(triangles).ToList();

                        var (min, max) = TrianglesUtility.GetMinMaxXYZFromMeshTriangles(triangles);
                        if (min != null && max != null)
                        {
                            double minDisToMin = double.MaxValue;
                            double minDisToMax = double.MaxValue;
                            XYZ minPointOfTriangles = null;
                            XYZ maxPointOfTriangles = null;
                            foreach (var p in points)
                            {
                                if (p.DistanceTo(min) < minDisToMin)
                                {
                                    minDisToMin = p.DistanceTo(min);
                                    minPointOfTriangles = p;
                                }
                            }
                            foreach (var p in points)
                            {
                                if (p.DistanceTo(max) < minDisToMax)
                                {
                                    minDisToMax = p.DistanceTo(max);
                                    maxPointOfTriangles = p;
                                }
                            }
                            if (minPointOfTriangles != null && maxPointOfTriangles != null)
                            {
                                var minMaxPairs = new List<XYZ> { minPointOfTriangles, maxPointOfTriangles };
                                XYZ centerPoint = PointUtility.GetCenterPoint(minMaxPairs);
                                double maxLenghOfLine = minPointOfTriangles.DistanceTo(maxPointOfTriangles);
                                Line line = LineUtility.CreateLine(centerPoint, axis, maxLenghOfLine);
                                XYZ minCenterPoint = LineUtility.GetPerpendicularProjectionPointOnLine(line, minPointOfTriangles);
                                XYZ maxCenterPoint = LineUtility.GetPerpendicularProjectionPointOnLine(line, maxPointOfTriangles);

                                Line newLine = Line.CreateBound(minCenterPoint, maxCenterPoint);
                                lines.Add(newLine);
                                //Line radiusLine = Line.CreateBound(maxCenterPoint, max);

                                //DrawPointLineArc.CreateModelLine(doc, minCenterPoint, maxCenterPoint, isRevitLink, transform, 0);
                                //DrawPointLineArc.CreateModelLine(doc, maxPointOfTriangles, maxCenterPoint, isRevitLink, transform, 0);
                            }
                        }
                    }

                    if (lines.Count >= 2)
                    {
                        Plane plane = null;
                        for (int i = 1; i < lines.Count; i++)
                        {
                            plane = LineUtility.CreatePlaneFromTwoLineAndIncludeOneLine(lines[0], lines[i], CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE);
                            if (plane != null) break;
                        }
                        if (plane != null)
                        {
                            var newLines = new List<Line> { lines[0] };

                            for (int i = 1; i < lines.Count; i++)
                            {
                                newLines.Add(LineUtility.CreateLineOnPlane(plane, lines[i]));
                            }
                            var dict = new Dictionary<Line, List<XYZ>>(new LineEqualityComparer());
                            for (int i = 0; i < newLines.Count - 1; i++)
                            {
                                XYZ dir1 = newLines[i].Direction.Normalize();
                                //List<XYZ> listInterectPoint = new List<XYZ>();

                                for (int j = i + 1; j < newLines.Count; j++)
                                {
                                    XYZ dir2 = newLines[i].Direction.Normalize();
                                    if (GeometryUtility.AreVectorsPerpendicular(dir1, dir2, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE))
                                    {
                                        XYZ intersectPoint = LineUtility.FindIntersectionFromLines(newLines[i], newLines[j]);
                                        if (intersectPoint != null)
                                        {
                                            //listInterectPoint.Add(intersectPoint);
                                            if (!LineUtility.IsPointBetweenTwoPointsOfLine(newLines[i], intersectPoint))
                                            {
                                                if (!dict.TryGetValue(newLines[i], out var list))
                                                {
                                                    list = new List<XYZ>();
                                                    dict[newLines[i]] = list;
                                                }

                                                list.Add(intersectPoint);
                                            }
                                            else if (!LineUtility.IsPointBetweenTwoPointsOfLine(newLines[j], intersectPoint))
                                            {
                                                if (!dict.TryGetValue(newLines[j], out var list))
                                                {
                                                    list = new List<XYZ>();
                                                    dict[newLines[j]] = list;
                                                }

                                                list.Add(intersectPoint);
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var keyValue in dict)
                            {
                                if (keyValue.Value.Count == 1)
                                {
                                    newLines.Remove(keyValue.Key);
                                    Line newLine = null;
                                    XYZ p1 = keyValue.Key.GetEndPoint(0);
                                    XYZ p2 = keyValue.Key.GetEndPoint(1);
                                    XYZ intersectPoint = keyValue.Value[0];
                                    double dis1 = intersectPoint.DistanceTo(p1);
                                    double dis2 = intersectPoint.DistanceTo(p2);

                                    if (dis1 < dis2)
                                    {
                                        newLine = Line.CreateUnbound(intersectPoint, p2);
                                    }
                                    else
                                    {
                                        newLine = Line.CreateUnbound(intersectPoint, p1);
                                    }
                                    newLines.Add(newLine);
                                }
                                else if (keyValue.Value.Count == 2)
                                {
                                    XYZ p1 = keyValue.Value[0];
                                    XYZ p2 = keyValue.Value[1];
                                    Line newLine = Line.CreateUnbound(p1, p2);
                                    newLines.Remove(keyValue.Key);
                                    newLines.Add(newLine);
                                }
                            }

                            DrawPointLineArc.DrawLines(doc, newLines, isRevitLink, transform, 0);
                        }
                    }
                    if (lines.Count == 1)
                    {
                        DrawPointLineArc.DrawLines(doc, lines, isRevitLink, transform, 0);
                    }
                }
            }
            return null;
        }

        private void GetCylindricalFaces(List<MeshTriangle> meshTriangles, List<(XYZ, List<MeshTriangle>)> groupTrianglesAndAxis, ref bool stop)
        {
            XYZ axis = null;
            MeshTriangle originTriangle = null;

            for (int i = 0; i < meshTriangles.Count - 1; i++)
            {
                XYZ normal1 = TrianglesUtility.GetNormalFromTriangle(meshTriangles[i]);
                for (int j = i + 1; j < meshTriangles.Count; j++)
                {
                    XYZ normal2 = TrianglesUtility.GetNormalFromTriangle(meshTriangles[j]);
                    if (TrianglesUtility.HasCommonVertex(meshTriangles[i], meshTriangles[j])
                        && !GeometryUtility.IsParallel(normal1, normal2, CommonConstants.TOLERANCE)
                        && !GeometryUtility.AreVectorsPerpendicular(normal1, normal2, CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE))
                    {
                        axis = normal1.CrossProduct(normal2).Normalize();
                        originTriangle = meshTriangles[i];

                        break;
                    }
                }
                if (axis != null) break;
            }

            if (axis != null)
            {
                GeometryUtility.IsParallelToAnyAxis(ref axis, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE);

                var trianglesParallelToVector = TrianglesUtility.GetTrianglesParallelToVector(axis, meshTriangles, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE).ToHashSet();
                //var list = TrianglesUtility.FindConnectedTrianglesByVertex(trianglesParallelToVector.ToList(), originTriangle).ToHashSet();
                var list = TrianglesUtility.FindConnectedTrianglesBySharedTwoVertex(trianglesParallelToVector.ToList(), originTriangle).ToHashSet();

                groupTrianglesAndAxis.Add((axis, list.ToList()));

                meshTriangles.RemoveAll(a => list.Contains(a));
                //if (groups.Count > 0)
                //{
                //    //TrianglesUtility.DrawHighestAndLowestTriangle(list.ToList(), doc, isRevitLink, transform);
                //    TrianglesUtility.DrawTriangles(list.ToList(), doc, isRevitLink, transform);
                //    //stop = true;
                //    TaskDialog.Show("Noti", axis.ToString());
                //}
            }
            else
            {
                stop = true;
            }
        }

        private void IsElementCylinderOrRectangle(List<MeshTriangle> meshTriangles, ref bool isCylinder, ref bool isRectangle)
        {
            for (int i = 0; i < meshTriangles.Count - 1; i++)
            {
                XYZ normal1 = TrianglesUtility.GetNormalFromTriangle(meshTriangles[i]);
                XYZ normal2 = TrianglesUtility.GetNormalFromTriangle(meshTriangles[i + 1]);
                if (!GeometryUtility.IsParallel(normal1, normal2, CommonConstants.TOLERANCE)
                    && !GeometryUtility.AreVectorsPerpendicular(normal1, normal2, CommonConstants.TOLERANCE))
                {
                    isCylinder = true;
                }
            }
            if (isCylinder == false)
            {
                isRectangle = true;
            }
        }

        private Solid MakeAxisHeigthForExtrusion(List<(XYZ, XYZ)> group1, List<(XYZ, XYZ)> group2)
        {
            XYZ randomPoint = group1[0].Item1;
            XYZ closestPoint = null;
            double minDistance = double.MaxValue;
            foreach (var (start, end) in group2)
            {
                if (randomPoint.DistanceTo(start) < minDistance)
                {
                    minDistance = randomPoint.DistanceTo(start);
                    closestPoint = start;
                }
            }
            if (closestPoint != null)
            {
                XYZ axis = (closestPoint - randomPoint).Normalize();
                double height = minDistance;
                var results = GroupClosedLoops(group1);
                Solid newsolid = SolidUtility.CreateNewSolidFromPoints(results, axis, height);
                return newsolid;
                //TestForDebug.CreateDirectShapeFromSolid(doc, newsolid);
            }
            return null;
        }

        private List<List<(XYZ start, XYZ end)>> GroupClosedLoops(List<(XYZ start, XYZ end)> segments)
        {
            var comparer = new XYZComparer();
            var remaining = new List<(XYZ start, XYZ end)>(segments);
            var result = new List<List<(XYZ start, XYZ end)>>();

            while (remaining.Count > 0)
            {
                var loop = new List<(XYZ start, XYZ end)>();

                // Lấy đoạn đầu
                var current = remaining[0];
                loop.Add(current);
                remaining.RemoveAt(0);

                while (true)
                {
                    var next = remaining.FirstOrDefault(s => comparer.Equals(s.start, current.end));

                    if (next == default) break;

                    loop.Add(next);
                    remaining.Remove(next);
                    current = next;

                    // Nếu khép kín thì kết thúc
                    if (comparer.Equals(current.end, loop[0].start))
                        break;
                }

                // Kiểm tra vòng có khép kín không
                if (!comparer.Equals(loop.Last().end, loop.First().start))
                    throw new InvalidOperationException("Tồn tại một chuỗi không khép kín.");

                result.Add(loop);
            }

            return result;
        }

        /// <summary>
        /// Chuẩn bị các thông số như danh sách các cặp điểm để tạo line
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="lineAndIntersectPointOnFaces">Danh sách các tuple trong đó là các nhóm (line, danh sách các giao điểm, CylindricalFace cần lấy bán kính) </param>
        /// <returns>Trả về danh sách chứa kich thước của 1 hình trụ, tông hợp cả danh sách là tất cả thông số kích thước của 1 solid </returns>
        private List<CylinderDimensions> PreparePointsForDrawingModelLine(Document doc, List<(Line, List<XYZ>, CylindricalFace)> lineAndIntersectPointOnFaces)
        {
            List<CylinderDimensions> listCylinderDimension = new List<CylinderDimensions>();

            foreach (var tupleValue in lineAndIntersectPointOnFaces)
            {
                CylinderDimensions cylinderDimensions = new CylinderDimensions();
                double radius = FaceUtility.GetRadius(tupleValue.Item3);
                if (tupleValue.Item2.Count == 2)
                {
                    Line newLine = Line.CreateBound(tupleValue.Item2[0], tupleValue.Item2[1]);
                    DrawPointLineArc.CreateModelLine(doc, tupleValue.Item2[0], tupleValue.Item2[1], isRevitLink, transform, 0);

                    cylinderDimensions.LengthLine = newLine;
                    cylinderDimensions.Radius = radius;
                }
                else if (tupleValue.Item2.Count == 1)
                {
                    foreach (var tuple in lineAndIntersectPointOnFaces)
                    {
                        double dot = tupleValue.Item1.Direction.Normalize().DotProduct(tuple.Item1.Direction.Normalize());
                        if (Math.Abs(dot) < CommonConstants.TOLERANCE)
                        {
                            XYZ point = tupleValue.Item2.FirstOrDefault();
                            XYZ projectedPoint = GetProjectedPoint(tupleValue.Item1.Direction.Normalize(), tuple.Item1, point);
                            Line newLine = Line.CreateBound(point, projectedPoint);
                            DrawPointLineArc.CreateModelLine(doc, point, projectedPoint, isRevitLink, transform, 0);

                            cylinderDimensions.LengthLine = newLine;
                            cylinderDimensions.Radius = radius;
                        }
                    }
                }
                else if (tupleValue.Item2.Count == 0)
                {
                    Line line = tupleValue.Item1;
                    XYZ pointOnLine = line.Evaluate(0.0, false);
                    List<XYZ> listProjectedPoints = new List<XYZ>();
                    foreach (var tuple in lineAndIntersectPointOnFaces)
                    {
                        double dot = tupleValue.Item1.Direction.Normalize().DotProduct(tuple.Item1.Direction.Normalize());
                        if (Math.Abs(dot) < CommonConstants.TOLERANCE)
                        {
                            XYZ projectedPoint = GetProjectedPoint(line.Direction.Normalize(), tuple.Item1, pointOnLine);
                            listProjectedPoints.Add(projectedPoint);
                        }
                    }
                    if (listProjectedPoints.Count == 2)
                    {
                        Line newLine = Line.CreateBound(listProjectedPoints[0], listProjectedPoints[1]);
                        DrawPointLineArc.CreateModelLine(doc, listProjectedPoints[0], listProjectedPoints[1], isRevitLink, transform, 0);
                        cylinderDimensions.LengthLine = newLine;
                        cylinderDimensions.Radius = radius;
                    }
                }
                listCylinderDimension.Add(cylinderDimensions);
            }
            return listCylinderDimension;
        }

        private List<(Line, List<XYZ>, CylindricalFace)> FindLinesAndIntersectionsOnFace(Document doc, List<PlanarFace> planarFaces, List<CylindricalFace> cylindricalFaces)
        {
            var lineAndIntersectPointOnFaces = new List<(Line, List<XYZ>, CylindricalFace)>();
            if (planarFaces.Count > 0)
            {
                foreach (var cylindricalFace in cylindricalFaces)
                {
                    XYZ vectorAxis = cylindricalFace.Axis;
                    XYZ originPoint = cylindricalFace.Origin;
                    Line line = Line.CreateUnbound(originPoint, vectorAxis.Normalize());
                    List<XYZ> points = new List<XYZ>();
                    foreach (var face in planarFaces)
                    {
                        XYZ point = FaceUtility.GetIntersectionPoint(line, face);
                        if (point != null)
                        {
                            points.Add(point);
                        }
                    }
                    lineAndIntersectPointOnFaces.Add((line, points, cylindricalFace));
                }
            }
            return lineAndIntersectPointOnFaces;
        }

        /// <summary>
        /// Lấy ra danh sách các CylindricalFace là đại diện cho từng khối hình trụ, 1 khối hình trụ có nhiều CylindricalFace
        /// và tât cả đều có chung axis và origin nên chỉ cần lấy ra 1 face để đại diện
        /// </summary>
        /// <param name="cylindricalFaces">danh sách các CylindricalFace của solid</param>
        /// <param name="planarFaces">danh sách các planarFaces của solid</param>
        /// <returns>Trả về 1 danh sách các CylindricalFace đại diện cho các khối hình trụ </returns>
        private List<CylindricalFace> GetCylindricalFaces(List<CylindricalFace> cylindricalFaces, List<PlanarFace> planarFaces)
        {
            var listCylindricalFace = new List<CylindricalFace>();
            if (cylindricalFaces.Count > 0)
            {
                if (cylindricalFaces.Count == 4 && planarFaces.Count == 2)
                {
                    listCylindricalFace.Add(cylindricalFaces.First());
                }
                else
                {
                    for (int i = 0; i < cylindricalFaces.Count - 1; i++)
                    {
                        for (int j = i + 1; j < cylindricalFaces.Count; j++)
                        {
                            if (cylindricalFaces[i].Origin.IsAlmostEqualTo(cylindricalFaces[j].Origin))
                            {
                                listCylindricalFace.Add(cylindricalFaces[i]);
                                break;
                            }
                        }
                    }
                }
            }
            return listCylindricalFace;
        }

        /// <summary>
        /// Lấy ra 1 điểm là điểm được chiếu từ 1 điểm đến 1 mặt phẳng
        /// </summary>
        /// <param name="normal">Vector chỉ phương của line chứa điểm cần chiếu</param>
        /// <param name="line">Mặt phẳng cần vẽ là mặt phẳng chứa line</param>
        /// <param name="point">Điểm cần chiếu</param>
        /// <returns>Trả về điểm chiếu trên mặt phẳng</returns>
        private XYZ GetProjectedPoint(XYZ normal, Line line, XYZ point)
        {
            XYZ pointOnLine = line.Evaluate(0.0, false);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, pointOnLine);

            XYZ planeOrigin = plane.Origin;
            XYZ planeNormal = plane.Normal.Normalize();
            XYZ pointToOrigin = point - planeOrigin;

            double distance = pointToOrigin.DotProduct(planeNormal);

            return point - distance * planeNormal;
        }

        /// <summary>
        /// Lấy ra danh sách tất cả các face của solid, gom nhóm các face đó theo planarFace,cylindricalFace
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid"></param>
        /// <returns>Trả về 1 tuple với item1 là danh sách các PlanarFace, item2 là danh sách các CylindricalFace,
        /// item3 là 1 dictionary chứa các cặp solid và danh sách face tương ứng</returns>
        private (List<PlanarFace>, List<CylindricalFace>, Dictionary<Solid, List<PlanarFace>>) GetGroupedFacesFromSolid(Document doc, Solid solid)
        {
            List<PlanarFace> planarFaces = new List<PlanarFace>();
            List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();
            Dictionary<Solid, List<PlanarFace>> solidPlanarFaces = new Dictionary<Solid, List<PlanarFace>>();

            if (IsRectangularBox(solid))
            {
                GetRectangularDimensions(doc, solid);
            }

            //var (planarFaces, cylindricalFaces) = SolidUtility.GetGroupedFacesFromSolid(doc, solid);

            //solidPlanarFaces.Add(solid, planarFaces);
            //return (planarFaces, cylindricalFaces, solidPlanarFaces);
            else if (solid.Faces.Size > 0 && solid.Volume > 0)
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace planarFace)
                    {
                        planarFaces.Add(planarFace);
                    }
                    else if (face is CylindricalFace cylindricalFace)
                    {
                        cylindricalFaces.Add(cylindricalFace);
                    }
                }
                solidPlanarFaces.Add(solid, planarFaces);
            }
            return (planarFaces, cylindricalFaces, solidPlanarFaces);
        }

        //End_______

        //__Hình hộp//
        //Start_______

        /// <summary>
        /// Hàm này chuẩn bị các thông số để tạo plane dùng cho việc cắt solid
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solidPlanarFaces">1 dictionary chứa các cặp solid và danh sách các face tương ứng</param>
        private void PrepareSolidCuttingData(Document doc, Dictionary<Solid, List<PlanarFace>> solidPlanarFaces)
        {
            foreach (var keyValue in solidPlanarFaces)
            {
                List<PlanarFace> facesWithFourEdges = new List<PlanarFace>();
                List<Edge> edgesOfSolid = new List<Edge>();

                foreach (Edge edge in keyValue.Key.Edges)
                {
                    edgesOfSolid.Add(edge);
                }

                foreach (var face in keyValue.Value)
                {
                    if (GetFaceEdgesWithCount(face).Num == 4)
                    {
                        facesWithFourEdges.Add(face);
                    }
                }

                PlanarFace faceOverFourEdges = null;
                foreach (var face in keyValue.Value)
                {
                    if (GetFaceEdgesWithCount(face).Num > 4)
                    {
                        faceOverFourEdges = face;
                        break;
                    }
                }

                var tupleValue = GetLongestEdgeAndParallelEdge(faceOverFourEdges);
                Plane plane = GetPlaneToCutSolid(doc, tupleValue.LongestEdge, tupleValue.ParallelEdge, facesWithFourEdges);
                if (plane != null)
                {
                    CutSolid(doc, keyValue.Key, plane);
                }
            }
        }

        /// <summary>
        /// Tìm ra cặp cạnh dài nhất và cạnh song song và gần nhất với cạnh dài nhất
        /// </summary>
        /// <param name="face">Là face có số cạnh lớn hơn 4</param>
        /// <returns>Trả về cặp cạnh dài nhất và cạnh gần với cạnh dài</returns>
        private (Edge LongestEdge, Edge ParallelEdge) GetLongestEdgeAndParallelEdge(PlanarFace face)
        {
            List<Edge> edges = GetFaceEdgesWithCount(face).Edges;
            Edge longestEdge = edges.OrderByDescending(e => e.ApproximateLength).First();

            return (longestEdge, FindClosestParallelEdge(longestEdge, edges));
        }

        /// <summary>
        /// Tìm cạnh song song và gần nhất với 1 cạnh cho trước
        /// </summary>
        /// <param name="targetEdge">Cạnh gần so</param>
        /// <param name="candidateEdges">Danh sách các cạnh của face có số cạnh lớn hơn 4</param>
        /// <returns>Trả về cạnh song song và gần nhất</returns>
        private Edge FindClosestParallelEdge(Edge targetEdge, List<Edge> candidateEdges)
        {
            if (targetEdge == null || candidateEdges == null || candidateEdges.Count == 0)
                return null;

            Curve targetCurve = targetEdge.AsCurve();
            XYZ targetDirection = (targetCurve.GetEndPoint(1) - targetCurve.GetEndPoint(0)).Normalize();

            Edge closestEdge = null;
            double minDistance = double.MaxValue;

            foreach (Edge candidate in candidateEdges)
            {
                if (candidate == targetEdge)
                    continue;

                Curve candidateCurve = candidate.AsCurve();
                XYZ candidateDirection = (candidateCurve.GetEndPoint(1) - candidateCurve.GetEndPoint(0)).Normalize();

                double dot = targetDirection.DotProduct(candidateDirection);
                if (Math.Abs(Math.Abs(dot) - 1.0) > CommonConstants.TOLERANCE)
                    continue;

                XYZ midTarget = (targetCurve.GetEndPoint(0) + targetCurve.GetEndPoint(1)) * 0.5;
                XYZ midCandidate = (candidateCurve.GetEndPoint(0) + candidateCurve.GetEndPoint(1)) * 0.5;
                double distance = midTarget.DistanceTo(midCandidate);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEdge = candidate;
                }
            }

            return closestEdge;
        }

        /// <summary>
        /// Thực hiện cắt solid bằng 1 plane, nếu phần còn thừa chưa phải là 1 solid hình hộp chữ nhật thì tiếp tục sử lý như ban đầu,
        /// cắt cho đến khi phần cuối cùng là 1 solid hình chữ nhật, sau đó sẽ có 1 danh sách các solid hình chữ nhật được cắt từ 1 khối solid ban đầu
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid">Khối solid cần cắt</param>
        /// <param name="plane">Mặt phẳng để cắt solid</param>
        private void CutSolid(Document doc, Solid solid, Plane plane)
        {
            Solid part = BooleanOperationsUtils.CutWithHalfSpace(solid, plane); // Phần trên LevelTop

            Plane flippedPlane = Plane.CreateByNormalAndOrigin(-plane.Normal, plane.Origin);
            Solid remaining = BooleanOperationsUtils.CutWithHalfSpace(solid, flippedPlane);

            GetRectangularDimensions(doc, part);
            Dictionary<Solid, List<PlanarFace>> solidPlanarFaces = new Dictionary<Solid, List<PlanarFace>>();

            IList<Solid> separatedSolids = SolidUtils.SplitVolumes(remaining);
            foreach (Solid s in separatedSolids)
            {
                if (IsRectangularBox(s))
                {
                    GetRectangularDimensions(doc, s);
                }
                else
                {
                    if (s.Faces.Size > 0 && s.Volume > 0)
                    {
                        List<PlanarFace> faces = new List<PlanarFace>();
                        foreach (Face face in s.Faces)
                        {
                            if (face is PlanarFace planarFace)
                            {
                                faces.Add(planarFace);
                            }
                        }
                        solidPlanarFaces.Add(s, faces);
                    }
                }
            }

            if (solidPlanarFaces.Count > 0)
            {
                PrepareSolidCuttingData(doc, solidPlanarFaces);
            }
        }

        /// <summary>
        /// Lấy ra kích thước của 1 solid có hình dạng là hình hộp chữ nhật, sau đó thêm vào trong danh sách
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid"></param>
        private void GetRectangularDimensions(Document doc, Solid solid)
        {
            RectangularDimensions rectangularDim = new RectangularDimensions();

            PlanarFace face1 = FaceUtility.GetSmallestFace(solid);
            PlanarFace face2 = null;
            foreach (Face face in solid.Faces)
            {
                if (!face.Equals(face1))
                {
                    if (face is PlanarFace planarFace)
                    {
                        if (FaceUtility.AreFacesParallel(face1, planarFace))
                        {
                            face2 = planarFace;
                            break;
                        }
                    }
                }
            }

            if (face1 != null && face2 != null)
            {
                XYZ point1 = FaceUtility.GetCenterOfFace(face1);
                XYZ point2 = FaceUtility.GetCenterOfFace(face2);
                var tupleValues = GetMidPointPairsOfRectangleFace(face1);
                if (tupleValues != null && tupleValues.Count == 2)
                {
                    rectangularDim.WidthLine = Line.CreateBound(tupleValues[0].Item1, tupleValues[0].Item2);
                    DrawPointLineArc.CreateModelLine(doc, tupleValues[0].Item1, tupleValues[0].Item2, isRevitLink, transform, 0);
                    rectangularDim.HeightLine = Line.CreateBound(tupleValues[1].Item1, tupleValues[1].Item2);
                    DrawPointLineArc.CreateModelLine(doc, tupleValues[1].Item1, tupleValues[1].Item2, isRevitLink, transform, 0);
                }

                rectangularDim.LengthLine = Line.CreateBound(point1, point2);
                DrawPointLineArc.CreateModelLine(doc, point1, point2, isRevitLink, transform, 0);
            }
            ListRectangularDimension.Add(rectangularDim);
        }

        /// <summary>
        /// Lấy ra các cặp điểm đối nhau, trong đó mỗi điểm là trung điểm của các cạnh trong 1 face
        /// </summary>
        /// <param name="face"></param>
        /// <returns>Trả về 1 danh sách của các cặp điểm đối nhau</returns>
        private List<(XYZ, XYZ)> GetMidPointPairsOfRectangleFace(Face face)
        {
            List<XYZ> midPoints = new List<XYZ>();
            EdgeArrayArray loops = face.EdgeLoops;
            if (loops.Size == 0)
                return null;

            EdgeArray outerLoop = loops.get_Item(0);
            foreach (Edge edge in outerLoop)
            {
                Curve curve = edge.AsCurve();
                XYZ mid = (curve.GetEndPoint(0) + curve.GetEndPoint(1)) * 0.5;
                midPoints.Add(mid);
            }

            if (midPoints.Count != 4)
                return null;

            List<(XYZ, XYZ)> result = new List<(XYZ, XYZ)>();

            for (int i = 0; i < midPoints.Count; i++)
            {
                for (int j = i + 1; j < midPoints.Count; j++)
                {
                    XYZ p1 = midPoints[i];
                    XYZ p2 = midPoints[j];
                    XYZ center = (p1 + p2) * 0.5;

                    var remaining = midPoints.Where((p, idx) => idx != i && idx != j).ToList();
                    if (remaining.Count == 2)
                    {
                        XYZ q1 = remaining[0];
                        XYZ q2 = remaining[1];
                        XYZ center2 = (q1 + q2) * 0.5;

                        if (center.IsAlmostEqualTo(center2))
                        {
                            result.Add((p1, p2));
                            result.Add((q1, q2));
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra xem solid có phải có dạng hình hộp chữ nhật không
        /// </summary>
        /// <param name="solid">Solid cần kiểm tra</param>
        /// <returns>Trả về true nếu solid là hình hộp chữ nhật và false nếu ngược lại</returns>
        private bool IsRectangularBox(Solid solid)
        {
            if (solid.Faces.Size == 6 && solid.Volume > 0 && solid.Edges.Size == 12)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Tạo 1 mặt phẳng để cắt solid từ face chứa cạnh song song và gần nhất với cạnh dài nhất được chọn
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="longestEdge">Cạnh dài nhất của 1 face có số cạnh lớn hơn 4, tức là face có hình dạng giống chữ </param>
        /// <param name="closestEdge">Cạnh song song và gần nhất với cạnh dài nhất</param>
        /// <param name="faces">Danh sách các face có 4 cạnh </param>
        /// <returns>Trả về 1 mặt phẳng được tạo từ face dùng để cắt solid</returns>
        private Plane GetPlaneToCutSolid(Document doc, Edge longestEdge, Edge closestEdge, List<PlanarFace> faces)
        {
            PlanarFace cutFace = null;
            foreach (var face in faces)
            {
                var tupleValues = GetFaceEdgesWithCount(face);
                if (tupleValues.Num == 4 && tupleValues.Edges.Contains(closestEdge))
                {
                    cutFace = face;
                    break;
                }
            }
            UV uv = new UV(0.5, 0.5);
            XYZ normal = cutFace.ComputeNormal(uv).Normalize();

            XYZ p = cutFace.Evaluate(uv);
            XYZ reversed = -normal;

            Plane plane = Plane.CreateByNormalAndOrigin(reversed, p);
            return plane;
        }

        /// <summary>
        /// Lấy ra danh sách và số lượng các cạnh của 1 face
        /// </summary>
        /// <param name="face"></param>
        /// <returns>Trả về 1 tuple với giả trị đầu tiên là danh sách các cạnh, giá trị thứ 2 là số lượng</returns>
        private (List<Edge> Edges, int Num) GetFaceEdgesWithCount(PlanarFace face)
        {
            EdgeArrayArray edgeArrays = face.EdgeLoops;
            List<Edge> listEdge = new List<Edge>();
            int sum = 0;
            foreach (EdgeArray edges in edgeArrays)
            {
                foreach (Edge edge in edges)
                {
                    listEdge.Add(edge);
                }
                sum += edges.Size;
            }

            return (listEdge, sum);
        }

        //End_________
    }

    public class RectangularDimensions
    {
        public Line LengthLine { get; set; }
        public Line WidthLine { get; set; }
        public Line HeightLine { get; set; }
    }

    public class CylinderDimensions
    {
        public Line LengthLine { get; set; }
        public double Radius { get; set; }
    }
}