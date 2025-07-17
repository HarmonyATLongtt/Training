using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

using Autodesk.Revit.UI;
using FirstCommand.Support.Constants;
using FirstCommand.Support.DebugTest;
using FirstCommand.Support.DrawOnRevit;
using FirstCommand.Support.FaceHandle;

using FirstCommand.Support.GenericClass.ComparerUtils;
using FirstCommand.Support.GeometryHandle;
using FirstCommand.Support.LineHandle;
using FirstCommand.Support.PlaneHandle;
using FirstCommand.Support.PointHandle;
using FirstCommand.Support.SolidHandle;
using FirstCommand.Support.TrianglesHandle;
using FirstCommand.Support.VectorHandle;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GetAxisFromLetterCommand : IExternalCommand
    {
        #region Propeties

        private Transform transform = null;

        //private bool isRevitLink = false;
        private List<RectangularDimensions> ListRectangularDimension = new List<RectangularDimensions>();

        private List<CylinderDimensions> ListCylinderDimension = new List<CylinderDimensions>();
        private Document doc;
        private TriangleVertexMap _data = null;

        #endregion Propeties

        #region Methods

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

                    List<PlanarFace> planarFaces = new List<PlanarFace>();
                    List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();
                    Dictionary<Solid, List<PlanarFace>> solidPlanarFaces = new Dictionary<Solid, List<PlanarFace>>();
                    List<MeshTriangle> meshTriangles = new List<MeshTriangle>();

                    var (solids, meshes) = GeometryUtility.GetSolids(linkedElem, doc);

                    bool isSolid = false;
                    bool isMesh = true;

                    //Thử trường hợp chuyển solid sang mesh
                    if (isMesh)
                    {
                        foreach (var s in solids)
                        {
                            meshTriangles.AddRange(TrianglesUtility.ExtractTrianglesFromSolid(s));
                        }
                    }

                    //Thử trường hợp solid
                    if (isSolid)
                    {
                        // Trường hợp geometry chỉ lấy được solid
                        if (solids.Count > 0 && meshes.Count == 0)
                        {
                            foreach (Solid s in solids)
                            {
                                var tupleValues = GetGroupedFacesFromSolid(doc, s);
                                planarFaces.AddRange(tupleValues.Item1);
                                cylindricalFaces.AddRange(tupleValues.Item2);
                                tupleValues.Item3.ToList().ForEach(kvp => solidPlanarFaces.Add(kvp.Key, kvp.Value));
                            }
                        }
                        // Trường hợp geometry lấy được meshes, có thể có solid
                        if (meshes.Count > 0)
                        {
                            foreach (var mesh in meshes)
                            {
                                meshTriangles.AddRange(TrianglesUtility.ExtractTrianglesFromMeshes(mesh));
                            }
                            if (solids.Count > 0)
                            {
                                foreach (var s in solids)
                                {
                                    meshTriangles.AddRange(TrianglesUtility.ExtractTrianglesFromSolid(s));
                                }
                            }
                        }

                        if (cylindricalFaces.Count > 0)
                        {
                            // với mesh thì không cần
                            var listCylindricalFaces = GetCylindricalFaces(cylindricalFaces, planarFaces);

                            var lineAndIntersectPointOnFaces = FindLinesAndIntersectionsOnFace(doc, planarFaces, listCylindricalFaces);

                            ListCylinderDimension = PreparePointsForDrawingModelLine(doc, lineAndIntersectPointOnFaces);
                        }
                        else
                        {
                            bool isCylinder = false;
                            if (planarFaces.Count > 0)
                            {
                                for (int i = 0; i < planarFaces.Count - 1; i++)
                                {
                                    XYZ normal1 = planarFaces[i].FaceNormal.Normalize();
                                    for (int j = i + 1; j < planarFaces.Count; j++)
                                    {
                                        XYZ normal2 = planarFaces[j].FaceNormal.Normalize();
                                        if (!VectorUtility.AreParallel(normal1, normal2, CommonConstants.TOLERANCE)
                                            && !VectorUtility.ArePerpendicular(normal1, normal2, CommonConstants.TOLERANCE))
                                        {
                                            isCylinder = true;
                                            break;
                                        }
                                    }
                                    if (isCylinder) break;
                                }
                            }

                            if (isCylinder)
                            {
                                meshTriangles.AddRange(TrianglesUtility.ExtractTrianglesFromFaces(planarFaces));
                            }
                            else
                            {
                                PrepareSolidCuttingData(doc, solidPlanarFaces);
                            }
                        }
                    }

                    if (meshTriangles.Count > 0)
                    {
                        bool isCylinder = false;
                        bool isRectangle = false;
                        IsElementCylinderOrRectangle(meshTriangles, ref isCylinder, ref isRectangle);

                        if (isRectangle)
                        {
                            var solid = CreateSolidFromMeshTriangles(meshTriangles);
                            if (solid != null)
                            {
                                var tupleValue = GetGroupedFacesFromSolid(doc, solid);

                                var newSolidAndPlanarFaces = tupleValue.Item3;
                                PrepareSolidCuttingData(doc, newSolidAndPlanarFaces);
                            }
                        }
                        else if (isCylinder)
                        {
                            DrawLinesAndRadiusOfCylinders(meshTriangles);
                        }
                    }

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

        /// <summary>
        /// Hàm dùng để vẽ trục và bán kính của các hình trụ
        /// </summary>
        /// <param name="meshTriangles"></param>
        private void DrawLinesAndRadiusOfCylinders(List<MeshTriangle> meshTriangles)
        {
            var groupAxisAndTriangles = new List<(XYZ, List<MeshTriangle>)>();
            int numOfTriangleMakePlanarFace = 3;

            // Xóa hết những nhóm tam giác tạo thành 1 mặt phẳng
            var meshTrianglesMakePlarnarFace = TrianglesUtility.GroupTrianglesByVertexAndNormal(meshTriangles, numOfTriangleMakePlanarFace).SelectMany(tri => tri).ToList();
            var setmeshTrianglesMakePlarnarFace = meshTrianglesMakePlarnarFace.ToHashSet();
            meshTriangles.RemoveAll(tr => setmeshTrianglesMakePlarnarFace.Contains(tr));

            bool stop = false;
            do
            {
                GroupTrianglesMakeCylinderAndGetAxis(meshTriangles, groupAxisAndTriangles, ref stop);
            }
            while (stop == false);

            if (groupAxisAndTriangles.Count > 0)
            {
                var lines = new List<Line>();
                var radiusAndLines = new Dictionary<Line, Line>(Comparers.Line);

                AddLinesCreatedByAxisAndRadiusFromMeshTriangles(lines, radiusAndLines, groupAxisAndTriangles);

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
                        var newLines = CreateNewLinesByProjectAllLinesOnAPlane(plane, lines, radiusAndLines);

                        var dictLineAndIntersectPoints = new Dictionary<Line, List<XYZ>>(Comparers.Line);
                        AddLineAndIntersectPointsToDict(newLines, dictLineAndIntersectPoints);

                        if (dictLineAndIntersectPoints.Count > 0)
                        {
                            UpDateNewLines(newLines, dictLineAndIntersectPoints, radiusAndLines);
                        }

                        foreach (var keyValue in radiusAndLines)
                        {
                            var pairLines = new List<Line> { keyValue.Key, keyValue.Value };
                            GetSolidFromCoupleLines(pairLines);
                            //DrawPointLineArc.DrawLines(doc, pairLines, isRevitLink, transform, 0);
                        }
                    }
                }
                if (lines.Count == 1)
                {
                    var firstPair = radiusAndLines.First();

                    var pairLines = new List<Line> { firstPair.Key, firstPair.Value };
                    GetSolidFromCoupleLines(pairLines);
                    //DrawPointLineArc.DrawLines(doc, pairLines, isRevitLink, transform, 0);
                }
            }
        }

        /// <summary>
        /// Tạo solid từ 1 cặp line đại diện cho 1 hình trụ(bán kính và trục)
        /// </summary>
        /// <param name="lines"></param>
        private void GetSolidFromCoupleLines(List<Line> lines)
        {
            if (lines.Count > 0)
            {
                Line linePresentForRadius = lines[0];
                Line linePresentForAxis = lines[1];
                Solid solid = SolidUtility.CreateCylindricalSolid(doc, linePresentForAxis.GetEndPoint(0), linePresentForRadius.Length, linePresentForAxis.Length, linePresentForAxis.Direction);
                TestForDebug.CreateDirectShapeFromSolid(doc, solid);
            }
        }

        private void UpDateNewLines(List<Line> newLines, Dictionary<Line, List<XYZ>> dictLineAndIntersectPoints, Dictionary<Line, Line> radiusAndLines)
        {
            foreach (var keyValue in dictLineAndIntersectPoints)
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
                        newLine = Line.CreateBound(intersectPoint, p2);
                    }
                    else
                    {
                        newLine = Line.CreateBound(intersectPoint, p1);
                    }
                    newLines.Add(newLine);
                    Line key = radiusAndLines.FirstOrDefault(x => x.Value == keyValue.Key).Key;
                    if (key != null)
                    {
                        radiusAndLines[key] = newLine;
                    }
                }
                else if (keyValue.Value.Count == 2)
                {
                    XYZ p1 = keyValue.Value[0];
                    XYZ p2 = keyValue.Value[1];
                    Line newLine = Line.CreateBound(p1, p2);
                    newLines.Remove(keyValue.Key);
                    newLines.Add(newLine);

                    Line key = radiusAndLines.FirstOrDefault(x => x.Value == keyValue.Key).Key;
                    if (key != null)
                    {
                        radiusAndLines[key] = newLine;
                    }
                }
            }
        }

        /// <summary>
        /// Hàm dùng để thêm line và các intersectPoints của line đó vào trong dict
        /// </summary>
        /// <param name="newLines"></param>
        /// <param name="dictLineAndIntersectPoints"></param>
        private void AddLineAndIntersectPointsToDict(List<Line> newLines, Dictionary<Line, List<XYZ>> dictLineAndIntersectPoints)
        {
            for (int i = 0; i < newLines.Count - 1; i++)
            {
                XYZ dir1 = newLines[i].Direction.Normalize();

                for (int j = i + 1; j < newLines.Count; j++)
                {
                    XYZ dir2 = newLines[j].Direction.Normalize();
                    if (VectorUtility.ArePerpendicular(dir1, dir2, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE))
                    {
                        XYZ intersectPoint = LineUtility.FindIntersectionFromLines(newLines[i], newLines[j]);
                        if (intersectPoint != null)
                        {
                            if (!LineUtility.IsPointBetweenTwoPointsOfLine(newLines[i], intersectPoint))
                            {
                                if (!dictLineAndIntersectPoints.TryGetValue(newLines[i], out var list))
                                {
                                    list = new List<XYZ>();
                                    dictLineAndIntersectPoints[newLines[i]] = list;
                                }

                                list.Add(intersectPoint);
                            }
                            else if (!LineUtility.IsPointBetweenTwoPointsOfLine(newLines[j], intersectPoint))
                            {
                                if (!dictLineAndIntersectPoints.TryGetValue(newLines[j], out var list))
                                {
                                    list = new List<XYZ>();
                                    dictLineAndIntersectPoints[newLines[j]] = list;
                                }

                                list.Add(intersectPoint);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Hàm dùng để tạo ra các line mới là hình chiếu của các line cũ lên trên 1 mặt phẳng, đồng thời cập nhật Dict radiusandlines
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="lines"></param>
        /// <param name="radiusAndLines"></param>
        /// <returns></returns>
        private List<Line> CreateNewLinesByProjectAllLinesOnAPlane(Plane plane, List<Line> lines, Dictionary<Line, Line> radiusAndLines)
        {
            var newLines = new List<Line> { lines[0] };

            for (int i = 1; i < lines.Count; i++)
            {
                Line newLine = LineUtility.CreateLineOnPlane(plane, lines[i]);
                if (newLine != null)
                {
                    newLines.Add(newLine);
                    Line key = radiusAndLines.FirstOrDefault(x => x.Value == lines[i]).Key;
                    if (key != null)
                    {
                        radiusAndLines[key] = newLine;
                    }
                }
            }
            return newLines;
        }

        /// <summary>
        /// Hàm dùng để lấy ra line và axis của tập hợp các tam giác tạo thành hình trụ
        /// và thêm vào trong danh sách
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="radiusAndLines"></param>
        /// <param name="groupAxisAndTriangles"></param>
        private void AddLinesCreatedByAxisAndRadiusFromMeshTriangles(List<Line> lines, Dictionary<Line, Line> radiusAndLines, List<(XYZ, List<MeshTriangle>)> groupAxisAndTriangles)
        {
            foreach (var (axis, triangles) in groupAxisAndTriangles)
            {
                var points = TrianglesUtility.GetVerticesOfTriangles(triangles);

                var (min, max) = TrianglesUtility.GetMinMaxXYZFromMeshTriangles(triangles);
                if (min != null && max != null)
                {
                    XYZ minPointOfTriangles = PointUtility.FindNearestPoint(points, min);
                    XYZ maxPointOfTriangles = PointUtility.FindNearestPoint(points, max);

                    if (minPointOfTriangles != null && maxPointOfTriangles != null)
                    {
                        var minMaxPairs = new List<XYZ> { minPointOfTriangles, maxPointOfTriangles };
                        XYZ centerPoint = PointUtility.GetCenterPoint(minMaxPairs);
                        double maxLenghOfLine = minPointOfTriangles.DistanceTo(maxPointOfTriangles);
                        Line line = LineUtility.CreateLine(centerPoint, axis, maxLenghOfLine);
                        XYZ minCenterPoint = LineUtility.GetPerpendicularProjectionPointOnLine(line, minPointOfTriangles);
                        XYZ maxCenterPoint = LineUtility.GetPerpendicularProjectionPointOnLine(line, maxPointOfTriangles);

                        //Test
                        //if (minCenterPoint.DistanceTo(maxCenterPoint) > 0.3 && maxPointOfTriangles.DistanceTo(maxCenterPoint) > 0.3)
                        //{
                        //    Line newLine = Line.CreateBound(minCenterPoint, maxCenterPoint);
                        //    Line radiusLine = Line.CreateBound(maxPointOfTriangles, maxCenterPoint);

                        //    radiusAndLines.Add(radiusLine, newLine);
                        //    lines.Add(newLine);
                        //}
                        //

                        Line newLine = Line.CreateBound(minCenterPoint, maxCenterPoint);
                        Line radiusLine = Line.CreateBound(maxPointOfTriangles, maxCenterPoint);

                        radiusAndLines.Add(radiusLine, newLine);
                        lines.Add(newLine);
                    }
                }
            }
        }

        /// <summary>
        /// Hàm dùng để tạo solid từ các meshtriangle
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <returns></returns>
        private Solid CreateSolidFromMeshTriangles(List<MeshTriangle> meshTriangles)
        {
            Solid solid = null;
            //Gom nhóm các triangle theo mặt phẳng
            int numOfTriangleMakePlanarFace = 2;
            var groupMeshTrianglesHasCommonVertexAndNormal = TrianglesUtility.GroupTrianglesByVertexAndNormal(meshTriangles, numOfTriangleMakePlanarFace);

            var pointsOfTrianglesFromGroupComplex = new List<List<(XYZ, XYZ)>>();
            var pointsOfTrianglesFromGroupSimple = new List<List<(XYZ, XYZ)>>();

            foreach (var triangles in groupMeshTrianglesHasCommonVertexAndNormal)
            {
                var xyzPairsMeshTrianglesMap = new Dictionary<UnorderedXYZPair, List<MeshTriangle>>();
                foreach (var tri in triangles)
                {
                    var XYZPairs = TrianglesUtility.GetXYZPairsOfTriangle(tri);
                    foreach (var pair in XYZPairs)
                    {
                        var newPair = new UnorderedXYZPair(pair.Item1, pair.Item2);
                        if (!xyzPairsMeshTrianglesMap.TryGetValue(newPair, out var list))
                        {
                            list = new List<MeshTriangle>();
                            xyzPairsMeshTrianglesMap[newPair] = list;
                        }

                        list.Add(tri);
                    }
                }
                // Danh sách các cạnh bên ngoài cùng
                var pairs = new List<UnorderedXYZPair>();
                foreach (var keyValue in xyzPairsMeshTrianglesMap)
                {
                    // Lấy những cặp XYZ chỉ thuộc 1 tam giác (tức là tạo thành cạnh ngoài cùng)
                    if (keyValue.Value.Count == 1)
                    {
                        pairs.Add(keyValue.Key);
                    }
                }

                var groupUnorderdXYZPairsHaveCommonPointAndMakeTraightLine = PointUtility.GroupColinearPairs(pairs);

                List<(XYZ, XYZ)> startEndPairs = new List<(XYZ, XYZ)>();
                foreach (var subGroup in groupUnorderdXYZPairsHaveCommonPointAndMakeTraightLine)
                {
                    var (startPoint, endPoint) = PointUtility.FindFurthestPointsInGroup(subGroup);
                    startEndPairs.Add((startPoint, endPoint));

                    // dùng startPoint và endPoint làm đầu – cuối đoạn thẳng đại diện
                }
                if (startEndPairs.Count == 4)
                {
                    pointsOfTrianglesFromGroupSimple.Add(startEndPairs.ToList());
                }
                else if (startEndPairs.Count > 4)
                {
                    pointsOfTrianglesFromGroupComplex.Add(startEndPairs.ToList());
                }
            }

            if (pointsOfTrianglesFromGroupSimple.Count >= 2 && pointsOfTrianglesFromGroupComplex.Count == 0)
            {
                HandleForSimpleCase(ref solid, pointsOfTrianglesFromGroupSimple);
            }
            else if (pointsOfTrianglesFromGroupComplex.Count == 2)
            {
                var group1 = pointsOfTrianglesFromGroupComplex[0];
                var group2 = pointsOfTrianglesFromGroupComplex[1];

                MakeAxisAndHeigthForExtrusion(ref solid, group1, group2);
            }
            return solid;
        }

        /// <summary>
        /// Xử lý cho trường hợp các cặp start end point tạo thành hình đơn giản (tạo thành 1 hình chữ nhật)
        /// </summary>
        /// <param name="pointsOfTrianglesFromGroupSimple"></param>
        /// <returns></returns>
        private void HandleForSimpleCase(ref Solid solid, List<List<(XYZ, XYZ)>> pointsOfTrianglesFromGroupSimple)
        {
            var group1 = pointsOfTrianglesFromGroupSimple[0];
            var group1Points = new HashSet<XYZ>(group1.SelectMany(pair => new[] { pair.Item1, pair.Item2 }), Comparers.XYZ);

            var group2 = pointsOfTrianglesFromGroupSimple
                .Skip(1)
                .FirstOrDefault(group => !group.Any(pair => group1Points.Contains(pair.Item1) || group1Points.Contains(pair.Item2)));
            if (group2 != default)
            {
                MakeAxisAndHeigthForExtrusion(ref solid, group1, group2);
            }
        }

        /// <summary>
        /// Hàm dùng để nhóm các nhóm tam giác tạo thành 1 hình trụ và lấy ra axis của trụ đó
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <param name="groupTrianglesAndAxis"></param>
        /// <param name="stop"></param>
        private void GroupTrianglesMakeCylinderAndGetAxis(List<MeshTriangle> meshTriangles, List<(XYZ, List<MeshTriangle>)> groupTrianglesAndAxis, ref bool stop)
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
                        && !VectorUtility.AreParallel(normal1, normal2, CommonConstants.TOLERANCE)
                        && !VectorUtility.ArePerpendicular(normal1, normal2, CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE))
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
                VectorUtility.IsParallelToAnyAxis(ref axis, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE);

                var trianglesParallelToVector = TrianglesUtility.GetTrianglesParallelToVector(_data, axis, meshTriangles, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE).ToHashSet();
                var list = TrianglesUtility.FindConnectedTrianglesBySharedTwoVertex(trianglesParallelToVector.ToList(), originTriangle).ToHashSet();

                groupTrianglesAndAxis.Add((axis, list.ToList()));
                meshTriangles.RemoveAll(a => list.Contains(a));
            }
            else
            {
                stop = true;
            }
        }

        /// <summary>
        /// Kiểm tra xem element là hình hộp chữ nhật hay là hình trụ
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <param name="isCylinder"></param>
        /// <param name="isRectangle"></param>
        private void IsElementCylinderOrRectangle(List<MeshTriangle> meshTriangles, ref bool isCylinder, ref bool isRectangle)
        {
            for (int i = 0; i < meshTriangles.Count - 1; i++)
            {
                XYZ normal1 = TrianglesUtility.GetNormalFromTriangle(meshTriangles[i]);
                XYZ normal2 = TrianglesUtility.GetNormalFromTriangle(meshTriangles[i + 1]);
                if (!VectorUtility.AreParallel(normal1, normal2, CommonConstants.TOLERANCE)
                    && !VectorUtility.ArePerpendicular(normal1, normal2, CommonConstants.TOLERANCE))
                {
                    isCylinder = true;
                }
            }
            if (isCylinder == false)
            {
                isRectangle = true;
            }
        }

        private void MakeAxisAndHeigthForExtrusion(ref Solid solid, List<(XYZ, XYZ)> group1, List<(XYZ, XYZ)> group2)
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

                solid = SolidUtility.CreateNewSolidFromPoints(results, axis, height);
            }
        }

        /// <summary>
        /// Hàm dùng để nhóm các cặp start end thành 1 vòng khép kín
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private List<List<(XYZ start, XYZ end)>> GroupClosedLoops(List<(XYZ start, XYZ end)> segments)
        {
            var comparer = Comparers.XYZ;
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
                    XYZ p1 = tupleValue.Item2[0];
                    XYZ p2 = tupleValue.Item2[1];
                    Line newLine = Line.CreateBound(p1, p2);

                    Solid solid = SolidUtility.CreateCylindricalSolid(doc, p1, radius, newLine.Length, newLine.Direction);
                    TestForDebug.CreateDirectShapeFromSolid(doc, solid);
                    //DrawPointLineArc.CreateModelLine(doc, p1,p2, isRevitLink, transform, 0);

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
                            //DrawPointLineArc.CreateModelLine(doc, point, projectedPoint, isRevitLink, transform, 0);
                            Solid solid = SolidUtility.CreateCylindricalSolid(doc, point, radius, newLine.Length, newLine.Direction);
                            TestForDebug.CreateDirectShapeFromSolid(doc, solid);

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
                        Solid solid = SolidUtility.CreateCylindricalSolid(doc, listProjectedPoints[0], radius, newLine.Length, newLine.Direction);
                        TestForDebug.CreateDirectShapeFromSolid(doc, solid);
                        //DrawPointLineArc.CreateModelLine(doc, listProjectedPoints[0], listProjectedPoints[1], isRevitLink, transform, 0);
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
            return PlaneUtility.GetProjectedPoint(plane, point);
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
                //GetRectangularDimensions(doc, solid);
            }
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
                    if (FaceUtility.GetEgdesAndNumOfFace(face).Num == 4)
                    {
                        facesWithFourEdges.Add(face);
                    }
                }

                PlanarFace faceOverFourEdges = null;
                foreach (var face in keyValue.Value)
                {
                    if (FaceUtility.GetEgdesAndNumOfFace(face).Num > 4)
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
            List<Edge> edges = FaceUtility.GetEgdesAndNumOfFace(face).Edges;
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
            //GetRectangularDimensions(doc, part);
            Dictionary<Solid, List<PlanarFace>> solidPlanarFaces = new Dictionary<Solid, List<PlanarFace>>();

            IList<Solid> separatedSolids = SolidUtils.SplitVolumes(remaining);
            foreach (Solid s in separatedSolids)
            {
                if (IsRectangularBox(s))
                {
                    //GetRectangularDimensions(doc, s);
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
        //private void GetRectangularDimensions(Document doc, Solid solid)
        //{
        //    RectangularDimensions rectangularDim = new RectangularDimensions();

        //    PlanarFace face1 = FaceUtility.GetSmallestFace(solid);
        //    PlanarFace face2 = null;
        //    foreach (Face face in solid.Faces)
        //    {
        //        if (!face.Equals(face1))
        //        {
        //            if (face is PlanarFace planarFace)
        //            {
        //                if (FaceUtility.AreFacesParallel(face1, planarFace))
        //                {
        //                    face2 = planarFace;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    if (face1 != null && face2 != null)
        //    {
        //        XYZ point1 = FaceUtility.GetCenterOfFace(face1);
        //        XYZ point2 = FaceUtility.GetCenterOfFace(face2);
        //        var tupleValues = GetMidPointPairsOfRectangleFace(face1);
        //        if (tupleValues != null && tupleValues.Count == 2)
        //        {
        //            rectangularDim.WidthLine = Line.CreateBound(tupleValues[0].Item1, tupleValues[0].Item2);
        //            DrawPointLineArc.CreateModelLine(doc, tupleValues[0].Item1, tupleValues[0].Item2,0, isRevitLink, transform);
        //            rectangularDim.HeightLine = Line.CreateBound(tupleValues[1].Item1, tupleValues[1].Item2);
        //            DrawPointLineArc.CreateModelLine(doc, tupleValues[1].Item1, tupleValues[1].Item2,0, isRevitLink, transform);
        //        }

        //        rectangularDim.LengthLine = Line.CreateBound(point1, point2);
        //        DrawPointLineArc.CreateModelLine(doc, point1, point2,0, isRevitLink, transform);
        //    }
        //    ListRectangularDimension.Add(rectangularDim);
        //}

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
                var tupleValues = FaceUtility.GetEgdesAndNumOfFace(face);
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

        #endregion Methods
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