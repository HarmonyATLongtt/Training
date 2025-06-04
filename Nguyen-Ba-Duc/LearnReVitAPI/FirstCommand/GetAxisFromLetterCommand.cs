using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GetAxisFromLetterCommand : IExternalCommand
    {
        private double tolerance = 1e-6;
        private Transform transform = null;
        private List<RectangularDimensions> ListRectangularDimension = new List<RectangularDimensions>();
        private List<CylinderDimensions> ListCylinderDimension = new List<CylinderDimensions>();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
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
                options.DetailLevel = ViewDetailLevel.Fine;
                options.ComputeReferences = true;
                GeometryElement elementGeo = linkedElem.get_Geometry(options);

                List<PlanarFace> planarFaces = new List<PlanarFace>();
                List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();
                Dictionary<Solid, List<PlanarFace>> solidPlanarFaces = new Dictionary<Solid, List<PlanarFace>>();
                //List<Solid> solids = new List<Solid>();
                foreach (GeometryObject geometryObj in elementGeo)
                {
                    if (geometryObj is Solid solid)
                    {
                        //solids.Add(solid);
                        var tupleValue = GetGroupedFacesFromSolid(doc, solid);
                        planarFaces = tupleValue.Item1;
                        cylindricalFaces = tupleValue.Item2;
                        solidPlanarFaces = tupleValue.Item3;
                    }
                    else if (geometryObj is GeometryInstance geomInstance)
                    {
                        GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                        foreach (GeometryObject geometryObject in instanceGeometry)
                        {
                            if (geometryObject is Solid nestedSolid)
                            {
                                //solids.Add(nestedSolid);
                                var tupleValue = GetGroupedFacesFromSolid(doc, nestedSolid);
                                planarFaces = tupleValue.Item1;
                                cylindricalFaces = tupleValue.Item2;
                                solidPlanarFaces = tupleValue.Item3;
                            }
                        }
                    }
                }

                //foreach (Solid solid in solids)
                //{
                //    var tupleValue = GetGroupedFacesFromSolid(doc, solid);
                //    if (tupleValue.Item1.Count > 0 && tupleValue.Item2.Count > 0 && tupleValue.Item3.Count > 0)
                //    {
                //        planarFaces.AddRange(tupleValue.Item1);
                //        cylindricalFaces.AddRange(tupleValue.Item2);
                //        solidPlanarFaces.AddRange(tupleValue.Item3);
                //    }
                //}

                if (cylindricalFaces.Count > 0)
                {
                    var listCylindricalFaces = GetCylindricalFaces(cylindricalFaces, planarFaces);

                    var lineAndIntersectPointOnFaces = FindLinesAndIntersectionsOnFace(doc, planarFaces, listCylindricalFaces);

                    ListCylinderDimension = PreparePointsForDrawingModelLine(doc, lineAndIntersectPointOnFaces);
                }
                else
                {
                    PrepareSolidCuttingData(doc, solidPlanarFaces);
                }

                var x = ListRectangularDimension;
                var y = ListCylinderDimension;
                return Result.Succeeded;
            }
            return Result.Failed;
        }

        //__Hình trụ//
        //Start_______

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
                double radius = GetRadius(tupleValue.Item3);
                if (tupleValue.Item2.Count == 2)
                {
                    Line newLine = CreateModelLine(doc, tupleValue.Item2[0], tupleValue.Item2[1]);

                    cylinderDimensions.LengthLine = newLine;
                    cylinderDimensions.Radius = radius;
                }
                else if (tupleValue.Item2.Count == 1)
                {
                    foreach (var tuple in lineAndIntersectPointOnFaces)
                    {
                        double dot = tupleValue.Item1.Direction.Normalize().DotProduct(tuple.Item1.Direction.Normalize());
                        if (Math.Abs(dot) < tolerance)
                        {
                            XYZ point = tupleValue.Item2.FirstOrDefault();
                            XYZ projectedPoint = GetProjectedPoint(tupleValue.Item1.Direction.Normalize(), tuple.Item1, point);
                            Line newLine = CreateModelLine(doc, point, projectedPoint);

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
                        if (Math.Abs(dot) < tolerance)
                        {
                            XYZ projectedPoint = GetProjectedPoint(line.Direction.Normalize(), tuple.Item1, pointOnLine);
                            listProjectedPoints.Add(projectedPoint);
                        }
                    }
                    if (listProjectedPoints.Count == 2)
                    {
                        Line newLine = CreateModelLine(doc, listProjectedPoints[0], listProjectedPoints[1]);
                        cylinderDimensions.LengthLine = newLine;
                        cylinderDimensions.Radius = radius;
                    }
                }
                listCylinderDimension.Add(cylinderDimensions);
            }
            return listCylinderDimension;
        }

        /// <summary>
        /// Lấy ra bán kính của 1 CylindricalFace
        /// </summary>
        /// <param name="face">Face cần lấy bán kính</param>
        /// <returns>Trả về 1 double là bán kính của face</returns>
        private double GetRadius(CylindricalFace face)
        {
            CylindricalSurface s = face.GetSurface() as CylindricalSurface;
            double radius = s.Radius;
            return radius;
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
                    Line line = CreateUnboundLine(originPoint, vectorAxis);
                    List<XYZ> points = new List<XYZ>();
                    foreach (var face in planarFaces)
                    {
                        XYZ point = GetIntersectionPoint(line, face);
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
        /// Hàm tạo 1 unbound line
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <returns>Trả về 1 unbound line</returns>
        private Line CreateUnboundLine(XYZ point, XYZ direction)
        {
            return Line.CreateUnbound(point, direction.Normalize());
        }

        /// <summary>
        /// Kiểm tra xem 1 unboundLine có giao với 1 face hay không
        /// </summary>
        /// <param name="unboundLine">là 1 line unbound</param>
        /// <param name="face"></param>
        /// <returns>Trả về 1 điểm nếu có giao nhau, không thì trả về null</returns>
        private XYZ GetIntersectionPoint(Line unboundLine, Face face)
        {
            IntersectionResultArray results;
            SetComparisonResult result = face.Intersect(unboundLine, out results);

            if (result == SetComparisonResult.Overlap && results != null && results.Size > 0)
            {
                return results.get_Item(0).XYZPoint;
            }
            return null;
        }

        /// <summary>
        /// Tạo modelline từ 2 điểm bất kỳ
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>Trả về line được tạo bởi 2 điểm</returns>
        private Line CreateModelLine(Document doc, XYZ point1, XYZ point2)
        {
            Line line = null;
            using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            {
                trans.Start();

                XYZ p1 = transform.OfPoint(point1);
                XYZ p2 = transform.OfPoint(point2);

                line = Line.CreateBound(p1, p2);
                XYZ direction = (p1 - p2).Normalize();

                bool isParallelToX = Math.Abs(direction.DotProduct(XYZ.BasisX)) > 0.99;
                bool isParallelToY = Math.Abs(direction.DotProduct(XYZ.BasisY)) > 0.99;
                bool isParallelToZ = Math.Abs(direction.DotProduct(XYZ.BasisZ)) > 0.99;

                Plane plane;

                if (isParallelToX)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisY, p1);
                }
                else if (isParallelToY)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else if (isParallelToZ)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else
                {
                    XYZ normal = direction.CrossProduct(XYZ.BasisZ).Normalize();

                    plane = Plane.CreateByNormalAndOrigin(normal, p1);
                }

                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                doc.Create.NewModelCurve(line, sketchPlane);

                trans.Commit();
            }
            return line;
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
                if (Math.Abs(Math.Abs(dot) - 1.0) > tolerance)
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

            PlanarFace face1 = GetSmallestFace(solid);
            PlanarFace face2 = null;
            foreach (Face face in solid.Faces)
            {
                if (!face.Equals(face1))
                {
                    if (face is PlanarFace planarFace)
                    {
                        if (AreFacesParallel(face1, planarFace))
                        {
                            face2 = planarFace;
                            break;
                        }
                    }
                }
            }

            if (face1 != null && face2 != null)
            {
                XYZ point1 = GetCenterOfFace(face1);
                XYZ point2 = GetCenterOfFace(face2);
                var tupleValues = GetMidPointPairsOfRectangleFace(face1);
                if (tupleValues != null && tupleValues.Count == 2)
                {
                    rectangularDim.WidthLine = CreateModelLine(doc, tupleValues[0].Item1, tupleValues[0].Item2);
                    rectangularDim.HeightLine = CreateModelLine(doc, tupleValues[1].Item1, tupleValues[1].Item2);
                }

                rectangularDim.LengthLine = CreateModelLine(doc, point1, point2);
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
        /// Kiểm tra xem 2 face có song song với nhau hay không
        /// </summary>
        /// <param name="face1"></param>
        /// <param name="face2"></param>
        /// <returns>Trả về true nếu song song và false nếu không</returns>
        private bool AreFacesParallel(PlanarFace face1, PlanarFace face2)
        {
            XYZ normal1 = face1.FaceNormal.Normalize();
            XYZ normal2 = face2.FaceNormal.Normalize();
            XYZ cross = normal1.CrossProduct(normal2);

            return cross.GetLength() < tolerance;
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

        /// <summary>
        /// Lấy ra face có diện tích nhỏ nhất
        /// </summary>
        /// <param name="solid">Tham số truyền vào là 1 solid</param>
        /// <returns>Trả về 1 face là face có diện tích nhỏ nhất</returns>
        private PlanarFace GetSmallestFace(Solid solid)
        {
            PlanarFace smallestFace = null;
            double minArea = double.MaxValue;
            foreach (Face face in solid.Faces)
            {
                if (face is PlanarFace planarFace)
                {
                    double area = planarFace.Area;
                    if (area < minArea)
                    {
                        minArea = area;
                        smallestFace = planarFace;
                    }
                }
            }
            return smallestFace;
        }

        /// <summary>
        /// Lấy ra tâm của 1 face
        /// </summary>
        /// <param name="face"></param>
        /// <returns>Trả về 1 điểm XYZ là tâm của 1 face</returns>
        private XYZ GetCenterOfFace(Face face)
        {
            BoundingBoxUV bbox = face.GetBoundingBox();
            UV centerUV = (bbox.Min + bbox.Max) * 0.5;
            XYZ centerXYZ = face.Evaluate(centerUV);
            return centerXYZ;
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