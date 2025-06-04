using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RectangularCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            //Application app = uiapp.Application;

            Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            if (r != null)
            {
                ElementId elementId = r.ElementId;
                Element element = doc.GetElement(elementId);
                if (element is FamilyInstance familyInstance)
                {
                    Dictionary<Solid, List<Face>> dictSolid_Faces = GetFacesOnGeometry(element, doc);

                    PrepareForCutSolid(doc, dictSolid_Faces);
                }
                else
                {
                    TaskDialog.Show("Kết quả", "Phần tử được chọn không phải là một FamilyInstance.");
                }
                return Result.Succeeded;
            }
            return Result.Failed;
        }

        //Hàm thực hiện transaction
        public void RunTransaction(Document doc, string transactionName, Action<Transaction> action)
        {
            using (Transaction trans = new Transaction(doc, transactionName))
            {
                trans.Start();

                FailureHandlingOptions options = trans.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new WarningSuppressor());
                trans.SetFailureHandlingOptions(options);

                action(trans); // Thực thi hành động trong Transaction

                trans.Commit();
            }
        }

        //public void DrawPolygonWithModelLines(Document doc, List<XYZ> convexHull)
        //{
        //    if (convexHull == null || convexHull.Count < 2)
        //        return;

        //    using (Transaction trans = new Transaction(doc, "Draw Convex Hull"))
        //    {
        //        trans.Start();

        //        // Lấy mặt phẳng phác thảo từ View hiện tại
        //        Autodesk.Revit.DB.View view = doc.ActiveView;
        //        SketchPlane sketchPlane = view.SketchPlane;

        //        if (sketchPlane == null)
        //        {
        //            // Nếu view chưa có SketchPlane, tạo mới tại Z = 0
        //            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
        //            sketchPlane = SketchPlane.Create(doc, plane);
        //            view.SketchPlane = sketchPlane;
        //        }

        //        // Vẽ các cạnh của polygon bằng ModelLine
        //        for (int i = 0; i < convexHull.Count; i++)
        //        {
        //            XYZ p1 = convexHull[i];
        //            XYZ p2 = convexHull[(i + 1) % convexHull.Count];
        //            Line line = Line.CreateBound(p1, p2);
        //            doc.Create.NewModelCurve(line, sketchPlane);
        //        }

        //        trans.Commit();
        //    }
        //}

        //public List<XYZ> GetSamplePoints()
        //{
        //    // Tạo danh sách các điểm trong hình chữ nhật (2D)
        //    List<XYZ> points = new List<XYZ>();

        //    // Ví dụ: Tạo các điểm xung quanh hình chữ nhật có kích thước 10x10
        //    for (int x = 0; x < 10; x++)
        //    {
        //        for (int y = 0; y < 10; y++)
        //        {
        //            points.Add(new XYZ(x, y, 0));  // Các điểm trong mặt phẳng XY
        //        }
        //    }

        //    // Bạn có thể thay đổi cách tạo điểm theo yêu cầu
        //    // Ví dụ: Các điểm ngẫu nhiên trong phạm vi nhất định:
        //    Random rand = new Random();
        //    for (int i = 0; i < 20; i++) // 20 điểm ngẫu nhiên
        //    {
        //        double x = rand.NextDouble() * 10;
        //        double y = rand.NextDouble() * 10;
        //        points.Add(new XYZ(x, y, 0));  // Các điểm ngẫu nhiên trong mặt phẳng XY
        //    }

        //    return points;
        //}

        //public List<XYZ> GetConvexHull(List<XYZ> points)
        //{
        //    if (points.Count < 3) return new List<XYZ>(points); // ít hơn 3 điểm thì không tạo được polygon

        //    // Bước 1: Tìm điểm thấp nhất (góc dưới trái nhất)
        //    XYZ p0 = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();

        //    // Bước 2: Sắp xếp các điểm còn lại theo góc với p0
        //    List<XYZ> sorted = points
        //        .Where(p => p != p0)
        //        .OrderBy(p => Math.Atan2(p.Y - p0.Y, p.X - p0.X))
        //        .ThenBy(p => p.DistanceTo(p0))
        //        .ToList();

        //    // Thêm p0 vào đầu danh sách
        //    sorted.Insert(0, p0);

        //    // Bước 3: Graham scan
        //    Stack<XYZ> hull = new Stack<XYZ>();
        //    hull.Push(sorted[0]);
        //    hull.Push(sorted[1]);

        //    for (int i = 2; i < sorted.Count; i++)
        //    {
        //        XYZ top = hull.Pop();
        //        while (hull.Count > 0 && Cross(hull.Peek(), top, sorted[i]) <= 0)
        //        {
        //            top = hull.Pop();
        //        }
        //        hull.Push(top);
        //        hull.Push(sorted[i]);
        //    }

        //    return hull.Reverse().ToList();
        //}

        //// Hàm xác định hướng quay giữa 3 điểm
        //private double Cross(XYZ a, XYZ b, XYZ c)
        //{
        //    // Cross product của vector AB và AC (chỉ xét mặt phẳng XY)
        //    double abX = b.X - a.X;
        //    double abY = b.Y - a.Y;
        //    double acX = c.X - a.X;
        //    double acY = c.Y - a.Y;
        //    return abX * acY - abY * acX;
        //}

        private void PrepareForCutSolid(Document doc, Dictionary<Solid, List<Face>> dictSolid_Faces)
        {
            foreach (var keyValue in dictSolid_Faces)
            {
                List<Face> facesHave4Edges = new List<Face>();
                List<Edge> listEdgesOfSolid = new List<Edge>();

                foreach (Edge edge in keyValue.Key.Edges)
                {
                    listEdgesOfSolid.Add(edge);
                }

                foreach (Face face in keyValue.Value)
                {
                    if (GetEgdesOfFace(face).Num == 4)
                    {
                        facesHave4Edges.Add(face);
                    }
                }

                Face faceOver4Edges = null;
                foreach (Face face in keyValue.Value)
                {
                    if (GetEgdesOfFace(face).Num > 4)
                    {
                        faceOver4Edges = face;
                        break;
                    }
                }

                //Edge longestEdge = LongestSharedEdge(listEdgesOfSolid, facesHave4Edges);
                //Edge longestEdge1 = GetEgdesOfFace(faceOver4Edges).Edges.OrderByDescending(e => e.ApproximateLength).First();
                var tupleValues = FindLongestEdgeAndParallelEdge(faceOver4Edges);
                Plane plane = GetFaceToCutSolid(doc, tupleValues.LongestEdge, tupleValues.ParallelEdge, facesHave4Edges);
                if (plane != null)
                {
                    CutSolid(doc, keyValue.Key, plane);
                }
            }
        }

        private (Edge LongestEdge, Edge ParallelEdge) FindLongestEdgeAndParallelEdge(Face face)
        {
            List<Edge> edges = GetEgdesOfFace(face).Edges;
            Edge longestEdge = edges.OrderByDescending(e => e.ApproximateLength).First();

            return (longestEdge, FindClosestParallelEdge(longestEdge, edges));
        }

        private Edge FindClosestParallelEdge(Edge targetEdge, List<Edge> candidateEdges, double angleTolerance = 1e-3)
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

                // Kiểm tra song song bằng DotProduct ≈ ±1
                double dot = targetDirection.DotProduct(candidateDirection);
                if (Math.Abs(Math.Abs(dot) - 1.0) > angleTolerance)
                    continue;

                // Tính khoảng cách giữa trung điểm hai cạnh
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

        private void CutSolid(Document doc, Solid solid, Plane plane)
        {
            Solid part = BooleanOperationsUtils.CutWithHalfSpace(solid, plane); // Phần trên LevelTop

            Plane flippedPlane = Plane.CreateByNormalAndOrigin(-plane.Normal, plane.Origin);
            Solid remaining = BooleanOperationsUtils.CutWithHalfSpace(solid, flippedPlane);

            CreateShapeFromSolid(doc, part);
            Dictionary<Solid, List<Face>> dictSolid_Faces = new Dictionary<Solid, List<Face>>();

            IList<Solid> separateSolids = SolidUtils.SplitVolumes(remaining);
            foreach (Solid s in separateSolids)
            {
                if (IsSolidRetanglar(s))
                {
                    CreateShapeFromSolid(doc, s);
                }
                else
                {
                    if (s.Faces.Size > 0 && s.Volume > 0)
                    {
                        List<Face> listFaces = new List<Face>();
                        foreach (Face face in s.Faces)
                        {
                            if (face is PlanarFace)
                            {
                                listFaces.Add(face);
                            }
                        }
                        dictSolid_Faces.Add(s, listFaces);
                    }
                }
            }

            if (dictSolid_Faces.Count > 0)
            {
                PrepareForCutSolid(doc, dictSolid_Faces);
            }
        }

        private void CreateShapeFromSolid(Document doc, Solid solid)
        {
            RunTransaction(doc, "Create New Direct Shape", (Transaction t) =>
            {
                var category = Category.GetCategory(doc, BuiltInCategory.OST_GenericModel);
                var ds = DirectShape.CreateElement(doc, category.Id);
                ds.SetShape(new List<GeometryObject> { solid });
            });
        }

        private bool IsSolidRetanglar(Solid solid)
        {
            if (solid.Faces.Size == 6 && solid.Volume > 0 && solid.Edges.Size == 12)
            {
                return true;
            }
            else return false;

            //if (solid.Faces.Size == 6 && solid.Volume > 0)
            //{
            //    bool result = true;
            //    foreach (Face face in solid.Faces)
            //    {
            //        if (face is PlanarFace)
            //        {
            //            if (GetEgdesOfFace(face).Num != 4)
            //            {
            //                result = false;
            //                return result;
            //            }
            //        }
            //    }
            //    return result;
            //}
            //return false;
        }

        private Face FindClosestFace(Edge edge, List<Face> facesB)
        {
            Face faceA = null;
            if (facesB == null || facesB.Count == 0)
                return null;

            foreach (Face face in facesB)
            {
                var pairs = GetEgdesOfFace(face);
                if (pairs.Edges.Contains(edge) && pairs.Num == 4)
                {
                    faceA = face;
                    facesB.Remove(face);
                    break;
                }
            }

            BoundingBoxUV bbox = faceA.GetBoundingBox();
            List<XYZ> samplePoints = new List<XYZ>
            {
                faceA.Evaluate(new UV(bbox.Min.U, bbox.Min.V)),
                faceA.Evaluate(new UV(bbox.Max.U, bbox.Max.V)),
                faceA.Evaluate(new UV((bbox.Min.U + bbox.Max.U) / 2, (bbox.Min.V + bbox.Max.V) / 2))
            };

            Face closestFace = null;
            double minDistance = double.MaxValue;
            List<Face> listFacesParallel = new List<Face>();
            foreach (Face faceB in facesB)
            {
                if (AreFacesParallel(faceA, faceB))
                {
                    listFacesParallel.Add(faceB);
                }
            }

            foreach (Face face in listFacesParallel)
            {
                double distance = ComputeMinimumDistance(face, samplePoints);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestFace = face;
                }
            }

            return closestFace;
        }

        private double ComputeMinimumDistance(Face face, List<XYZ> points)
        {
            double minDist = double.MaxValue;

            foreach (XYZ point in points)
            {
                IntersectionResult result = face.Project(point);
                if (result != null)
                {
                    double dist = point.DistanceTo(result.XYZPoint);
                    if (dist < minDist)
                        minDist = dist;
                }
            }

            return minDist;
        }

        private Plane GetFaceToCutSolid(Document doc, Edge longestEdge, Edge closestEdge, List<Face> listFaces)
        {
            //Edge edge = tupleValues.OrderByDescending(t => t.Item3.ApproximateLength).FirstOrDefault().Item3;

            //Face face = faces[0].Area < faces[1].Area ? faces[0] : faces[1];

            //Face cutFace = FindClosestFace(longestEdge, listFaces);
            Face cutFace = null;
            foreach (Face face in listFaces)
            {
                var tupleValues = GetEgdesOfFace(face);
                if (tupleValues.Num == 4 && tupleValues.Edges.Contains(closestEdge))
                {
                    cutFace = face;
                    break;
                }
            }

            //DrawFaceEdgesAsModelLines(doc, cutFace);

            UV uv = new UV(0.5, 0.5);
            XYZ normal = cutFace.ComputeNormal(uv).Normalize();

            XYZ p = cutFace.Evaluate(uv);
            XYZ reversed = -normal;

            Plane plane = Plane.CreateByNormalAndOrigin(reversed, p);
            //Plane plane = Plane.CreateByNormalAndOrigin(normal, p);
            return plane;

            //foreach (Face f in listFaces)
            //{
            //    foreach (Edge ed in GetEgdesOfFace(f).ListEdges)
            //    {
            //        Curve curve = ed.AsCurve();
            //        if (ed != edge && IsIntersect(face, curve))
            //        {
            //            UV uv = new UV(0.5, 0.5);

            //            XYZ normal = f.ComputeNormal(uv).Normalize();

            //            XYZ p = f.Evaluate(uv);
            //            XYZ reversed = -normal;

            //            Plane plane = Plane.CreateByNormalAndOrigin(reversed, p);
            //            return plane;
            //        }
            //    }
            //}
        }

        //private (List<Face> FacePairs, Edge edge, List<Face> ListFaces) GetFaceToCreateRectangle(List<Face> faces, List<Edge> listEdgesOfSolid)
        //{
        //    List<(Face, Face)> listFacesParallel = new List<(Face, Face)>();

        //    for (int i = 0; i < faces.Count - 1; i++)
        //    {
        //        for (int j = i + 1; j < faces.Count; j++)
        //        {
        //            if (AreFacesParallel(faces[i], faces[j]))
        //            {
        //                listFacesParallel.Add((faces[i], faces[j]));
        //            }
        //        }
        //    }

        //    var tupleValues = GetFacePairsWithSharedEdges(listEdgesOfSolid);
        //    //Dictionary<List<Face>, Edge> distanceOfTwoFaces = new Dictionary<List<Face>, Edge>();

        //    //foreach (var keyValue in GetFacePairsWithSharedEdges(listEdgesOfSolid))
        //    //{
        //    //    List<Face> listFaces = new List<Face>();
        //    //    if (keyValue.Value.ApproximateLength < DistanceOfPointToFace(keyValue.Key[0], keyValue.Key[1]))
        //    //    {
        //    //        listFaces.Add(keyValue.Key[0]);
        //    //        listFaces.Add(keyValue.Key[1]);
        //    //        distanceOfTwoFaces.Add(listFaces, keyValue.Value);
        //    //    }
        //    //}
        //    //var pairs = distanceOfTwoFaces.OrderByDescending(a => a.Value.ApproximateLength)
        //    //        .First();
        //    return ()
        //    return (pairs.Key, pairs.Value, faces);
        //}

        //private Dictionary<List<Face>, Edge> GetFacePairsWithSharedEdges(List<Edge> edges)
        private Edge LongestSharedEdge(List<Edge> edges, List<Face> facesHave4Edges)
        {
            //var result = new List<(Face, Face, Edge)>();
            List<Edge> listSharedEdges = new List<Edge>();
            var seenFaces = new HashSet<(int, int)>();
            foreach (Edge edge in edges)
            {
                Face face1 = edge.GetFace(0);
                Face face2 = edge.GetFace(1);
                if (face1 == null || face2 == null)
                    continue;
                if (!facesHave4Edges.Contains(face1) && !facesHave4Edges.Contains(face2))
                    continue;
                if (!AreFacesParallel(face1, face2))
                    continue;
                int id1 = face1.Id;
                int id2 = face2.Id;
                var key = id1 < id2 ? (id1, id2) : (id2, id1);

                if (seenFaces.Contains(key))
                    continue;

                Curve curve = edge.AsCurve();

                if (IsLineVerticalToFace(curve, face1))
                {
                    //result.Add((face1, face2, edge));
                    listSharedEdges.Add(edge);
                    seenFaces.Add(key);
                }
            }
            return listSharedEdges.OrderByDescending(e => e.ApproximateLength).FirstOrDefault();

            //Dictionary<List<Face>, Edge> keyValuePairs = new Dictionary<List<Face>, Edge>();
            //foreach (var pairs in listFacesParallel)
            //{
            //    List<Face> facePairs = new List<Face>();

            //    foreach (Edge edge in edges)
            //    {
            //        Face face1 = edge.GetFace(0);
            //        Curve curve = edge.AsCurve();
            //        {
            //            //if (IsLineVerticalToFace(curve, pairs.Item1) && IsLineVerticalToFace(curve, pairs.Item2) && IsIntersect(pairs.Item1, curve) && IsIntersect(pairs.Item2, curve))
            //            //{
            //            //    facePairs.Add(pairs.Item1);
            //            //    facePairs.Add(pairs.Item2);
            //            //    keyValuePairs.Add(facePairs, edge);
            //            //    break;
            //            //}
            //        }
            //    }
            //}
            //return keyValuePairs;
        }

        private bool IsLineVerticalToFace(Curve curve, Face face, double tolerance = 1e-6)
        {
            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();

            UV uv = new UV(0.5, 0.5);
            XYZ faceNormal = face.ComputeNormal(uv).Normalize();

            double dot = lineDirection.DotProduct(faceNormal);

            return Math.Abs(dot) < tolerance;
        }

        private bool IsIntersect(Face face, Curve curve, double tolerance = 1e-6)
        {
            XYZ startPoint = curve.GetEndPoint(0);
            XYZ endPoint = curve.GetEndPoint(1);

            var result1 = face.Project(startPoint);
            var result2 = face.Project(endPoint);

            bool intersectsStart = result1 != null && result1.XYZPoint.DistanceTo(startPoint) < tolerance;
            bool intersectsEnd = result2 != null && result2.XYZPoint.DistanceTo(endPoint) < tolerance;

            return intersectsStart || intersectsEnd;
        }

        //private bool IsIntersect(Face face, Curve curve)
        //{
        //    XYZ endPoint = curve.GetEndPoint(1); // điểm cuối
        //    XYZ startPoint = curve.GetEndPoint(0); // điểm đầu
        //    IntersectionResult result1 = face.Project(startPoint);
        //    IntersectionResult result2 = face.Project(endPoint);

        //    if (result1 != null || result2 != null)
        //    {
        //        return true;
        //    }
        //    return false;
        //    //IntersectionResultArray results;
        //    //SetComparisonResult comparisonResult = face.Intersect(curve, out results);

        //    //if (comparisonResult == SetComparisonResult.Overlap && results != null)
        //    //{
        //    //    return true;
        //    //}
        //    //return false;
        //}

        private double DistanceOfPointToFace(Face face1, Face face2)
        {
            UV uv = new UV(0.5, 0.5);

            XYZ normal1 = face1.ComputeNormal(uv).Normalize();
            XYZ normal2 = face2.ComputeNormal(uv).Normalize();

            XYZ p1 = face1.Evaluate(uv);
            XYZ p2 = face2.Evaluate(uv);

            Plane plane2 = Plane.CreateByNormalAndOrigin(normal2, p2);

            XYZ testPoint = p1 + normal1 * 1;

            XYZ vector = testPoint - p2;

            double distance = Math.Abs(vector.DotProduct(normal2));
            return distance;
        }

        private bool AreFacesParallel(Face face1, Face face2, double tolerance = 1e-6)
        {
            UV uv = new UV(0.5, 0.5);

            XYZ normal1 = face1.ComputeNormal(uv).Normalize();
            XYZ normal2 = face2.ComputeNormal(uv).Normalize();

            double dot = normal1.DotProduct(normal2);

            return Math.Abs(dot) - 1 < tolerance;
            //return Math.Abs(dot + 1) < tolerance;
        }

        private (List<Edge> Edges, int Num) GetEgdesOfFace(Face face)
        {
            EdgeArrayArray edgeArrays = face.EdgeLoops;

            //EdgeArray edges = edgeArrays.get_Item(0);
            List<Edge> listEdges = new List<Edge>();
            int sum = 0;
            foreach (EdgeArray edges in edgeArrays)
            {
                foreach (Edge edge in edges)
                {
                    listEdges.Add(edge);
                }
                sum += edges.Size;
            }
            return (listEdges, sum);
        }

        private Dictionary<Solid, List<Face>> GetFacesOnGeometry(Element element, Document doc)
        {
            Dictionary<Solid, List<Face>> keyValuePairs = new Dictionary<Solid, List<Face>>();

            FamilyInstance familyInstance = element as FamilyInstance;
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            foreach (GeometryObject geometryObj in elementGeo)
            {
                List<Face> listFaces = new List<Face>();
                if (geometryObj is Solid solid)
                {
                    if (IsSolidRetanglar(solid))
                    {
                        CreateShapeFromSolid(doc, solid);
                    }
                    else if (solid.Faces.Size > 0 && solid.Volume > 0)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace)
                            {
                                listFaces.Add(face);
                            }
                        }
                        keyValuePairs.Add(solid, listFaces);
                    }
                }
                else if (geometryObj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject in instanceGeometry)
                    {
                        if (geometryObject is Solid nestedSolid)
                        {
                            if (IsSolidRetanglar(nestedSolid))
                            {
                                CreateShapeFromSolid(doc, nestedSolid);
                            }
                            else if (nestedSolid.Faces.Size > 0 && nestedSolid.Volume > 0)
                            {
                                foreach (Face face in nestedSolid.Faces)
                                {
                                    if (face is PlanarFace)
                                    {
                                        listFaces.Add(face);
                                    }
                                }
                                keyValuePairs.Add(nestedSolid, listFaces);
                            }
                        }
                    }
                }
            }
            return keyValuePairs;
        }
    }
}