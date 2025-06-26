using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstCommand.Support;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CylinderCommand : IExternalCommand
    {
        private double tolerance = 1e-6;
        private Transform transform = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            //Application app = uiapp.Application;

            //Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.LinkedElement);
            if (r != null)
            {
                // Lấy link instance
                Element linkInstance = uidoc.Document.GetElement(r.ElementId);
                RevitLinkInstance rli = linkInstance as RevitLinkInstance;

                // Lấy Document bên trong Revit Link
                Document linkDoc = rli.GetLinkDocument();

                // Lấy ElementId bên trong link
                ElementId linkedElemId = r.LinkedElementId;

                // Lấy element thực sự trong Revit Link
                Element linkedElem = linkDoc.GetElement(linkedElemId);

                transform = rli.GetTransform();

                //ElementId elementId = r.ElementId;
                //Element element = doc.GetElement(elementId);

                var tupleValue = GetFacesOnGeometry(linkedElem, doc);
                List<PlanarFace> planarFaces = tupleValue.PlanarFaces;
                List<CylindricalFace> cylindricalFaces = tupleValue.CylindricalFaces;

                var listCylindricalFaces = GetListCylindricalFaces(cylindricalFaces, planarFaces);

                var listLineAndIntersectPointWithFace = GetListLineAndIntersectPointWithFace(doc, planarFaces, listCylindricalFaces);

                PrepairPointsToDrawModelLine(doc, listLineAndIntersectPointWithFace);

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

        //private void CreateShapeFromSolid(Document doc, List<Solid> listSolids)
        //{
        //    RunTransaction(doc, "Create New Direct Shape", (Transaction t) =>
        //    {
        //        var category = Category.GetCategory(doc, BuiltInCategory.OST_GenericModel);
        //        var ds = DirectShape.CreateElement(doc, category.Id);

        //        Solid merged = listSolids[0]; // bắt đầu từ solid đầu tiên

        //        for (int i = 1; i < listSolids.Count; i++)
        //        {
        //            merged = BooleanOperationsUtils.ExecuteBooleanOperation(
        //                merged,
        //                listSolids[i],
        //                BooleanOperationsType.Union);
        //        }
        //        ds.SetShape(new List<GeometryObject> { merged });
        //    });
        //}

        private void PrepairPointsToDrawModelLine(Document doc, List<(Line, List<XYZ>)> listLineAndIntersectPointWithFace)
        {
            foreach (var tupleValue in listLineAndIntersectPointWithFace)
            {
                if (tupleValue.Item2.Count == 2)
                {
                    CreateModelLineAutoPlane(doc, tupleValue.Item2[0], tupleValue.Item2[1]);
                }
                if (tupleValue.Item2.Count == 1)
                {
                    foreach (var tuple in listLineAndIntersectPointWithFace)
                    {
                        double dot = tupleValue.Item1.Direction.Normalize().DotProduct(tuple.Item1.Direction.Normalize());
                        if (Math.Abs(dot) < tolerance)
                        {
                            XYZ point = tupleValue.Item2.FirstOrDefault();
                            XYZ projectedPoint = GetProjectedPoint(tupleValue.Item1.Direction.Normalize(), tuple.Item1, point);
                            CreateModelLineAutoPlane(doc, point, projectedPoint);
                        }
                    }
                }
                if (tupleValue.Item2.Count == 0)
                {
                    Line line = tupleValue.Item1;
                    XYZ pointOnLine = line.Evaluate(0.0, false);
                    List<XYZ> listProjectedPoints = new List<XYZ>();
                    foreach (var tuple in listLineAndIntersectPointWithFace)
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
                        CreateModelLineAutoPlane(doc, listProjectedPoints[0], listProjectedPoints[1]);
                    }
                }
            }
        }

        private List<(Line, List<XYZ>)> GetListLineAndIntersectPointWithFace(Document doc, List<PlanarFace> planarFaces, List<CylindricalFace> listCylindricalFaces)
        {
            var listLineAndIntersectPointWithFace = new List<(Line, List<XYZ>)>();
            if (planarFaces.Count > 0)
            {
                foreach (var cylindricalFace in listCylindricalFaces)
                {
                    XYZ vectorAxis = cylindricalFace.Axis;
                    XYZ originPoint = cylindricalFace.Origin;
                    Line line = CreateUnboundLineFromPointAndVector(originPoint, vectorAxis);
                    List<XYZ> listPoints = new List<XYZ>();
                    foreach (var face in planarFaces)
                    {
                        XYZ point = IntersectLineWithFace(line, face);
                        if (point != null)
                        {
                            listPoints.Add(point);

                            //CurveLoop curveLoop = new CurveLoop();
                            //EdgeArrayArray edgeArrays = face.EdgeLoops;

                            //foreach (EdgeArray edges in edgeArrays)
                            //{
                            //    foreach (Edge edge in edges)
                            //    {
                            //        curveLoop.Append(edge.AsCurve());
                            //    }
                            //}
                            //List<CurveLoop> profileLoops = new List<CurveLoop> { curveLoop };

                            //Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(profileLoops, line.Direction, 5);
                            //if (solid != null)
                            //{
                            //    CreateShapeFromSolid(doc, solid);

                            //}
                        }
                    }
                    listLineAndIntersectPointWithFace.Add((line, listPoints));
                }
            }
            return listLineAndIntersectPointWithFace;
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

        private List<CylindricalFace> GetListCylindricalFaces(List<CylindricalFace> cylindricalFaces, List<PlanarFace> planarFaces)
        {
            var listCylindricalFaces = new List<CylindricalFace>();
            if (cylindricalFaces.Count > 0)
            {
                if (cylindricalFaces.Count == 4 && planarFaces.Count == 2)
                {
                    listCylindricalFaces.Add(cylindricalFaces.First());
                }
                else
                {
                    for (int i = 0; i < cylindricalFaces.Count - 1; i++)
                    {
                        for (int j = i + 1; j < cylindricalFaces.Count; j++)
                        {
                            if (cylindricalFaces[i].Origin.IsAlmostEqualTo(cylindricalFaces[j].Origin))
                            {
                                listCylindricalFaces.Add(cylindricalFaces[i]);
                                break;
                            }
                        }
                    }
                }
            }
            return listCylindricalFaces;
        }

        private bool ArePlanarFacesParallel(PlanarFace face1, PlanarFace face2)
        {
            // Lấy Normal vector của mỗi mặt
            XYZ normal1 = face1.FaceNormal.Normalize();
            XYZ normal2 = face2.FaceNormal.Normalize();

            // Lấy tích có hướng
            XYZ cross = normal1.CrossProduct(normal2);

            // Nếu độ dài tích có hướng ≈ 0, thì 2 vector song song hoặc ngược hướng
            return cross.GetLength() < tolerance;
        }

        private bool Are2VectorsParallel(XYZ vector1, XYZ vector2)
        {
            return vector1.CrossProduct(vector2).GetLength() < tolerance;
        }

        private Line CreateUnboundLineFromPointAndVector(XYZ point, XYZ direction)
        {
            return Line.CreateUnbound(point, direction.Normalize());
        }

        private XYZ IntersectLineWithFace(Line unboundLine, Face face)
        {
            IntersectionResultArray results;
            SetComparisonResult result = face.Intersect(unboundLine, out results);

            if (result == SetComparisonResult.Overlap && results != null && results.Size > 0)
            {
                return results.get_Item(0).XYZPoint;
            }

            return null;
        }

        private void CreateModelLineAutoPlane(Document doc, XYZ point1, XYZ point2)
        {
            using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            {
                trans.Start();

                XYZ newVector = -XYZ.BasisY.Multiply(10);
                XYZ pt1 = point1.Add(newVector);
                XYZ pt2 = point2.Add(newVector);
                XYZ p1 = transform.OfPoint(pt1);
                XYZ p2 = transform.OfPoint(pt2);

                Line line = Line.CreateBound(p1, p2);
                XYZ direction = (p1 - p2).Normalize();

                // Xác định xem vector direction có gần song song với các trục X, Y, Z hay không
                bool isParallelToX = Math.Abs(direction.DotProduct(XYZ.BasisX)) > 0.99;
                bool isParallelToY = Math.Abs(direction.DotProduct(XYZ.BasisY)) > 0.99;
                bool isParallelToZ = Math.Abs(direction.DotProduct(XYZ.BasisZ)) > 0.99;

                Plane plane;

                if (isParallelToX)
                {
                    // Song song trục X → dùng mặt phẳng YZ
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisY, p1);
                }
                else if (isParallelToY)
                {
                    // Song song trục Y → dùng mặt phẳng XZ
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else if (isParallelToZ)
                {
                    // Song song trục Z → dùng mặt phẳng XY
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else
                {
                    // Không song song với X/Y/Z → dùng trục Z để tạo normal qua cross product
                    XYZ normal = direction.CrossProduct(XYZ.BasisZ).Normalize();

                    plane = Plane.CreateByNormalAndOrigin(normal, p1);
                }

                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                doc.Create.NewModelCurve(line, sketchPlane);

                trans.Commit();
            }
        }

        private XYZ GetProjectedPoint(XYZ normal, Line line, XYZ point)
        {
            XYZ pointOnLine = line.Evaluate(0.0, false);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, pointOnLine);

            XYZ planeOrigin = plane.Origin;
            XYZ planeNormal = plane.Normal.Normalize(); // đảm bảo là vector đơn vị
            XYZ pointToOrigin = point - planeOrigin;

            // Tính khoảng cách từ điểm đến mặt phẳng (dọc theo pháp tuyến)
            double distance = pointToOrigin.DotProduct(planeNormal);

            // Chiếu điểm xuống mặt phẳng
            return point - distance * planeNormal;
        }

        private (List<PlanarFace> PlanarFaces, List<CylindricalFace> CylindricalFaces) GetFacesOnGeometry(Element element, Document doc)
        {
            FamilyInstance familyInstance = element as FamilyInstance;
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            List<PlanarFace> planarFaces = new List<PlanarFace>();
            List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();

            foreach (GeometryObject geometryObj in elementGeo)
            {
                List<Face> listFaces = new List<Face>();
                if (geometryObj is Solid solid)
                {
                    if (solid.Faces.Size > 0 && solid.Volume > 0)
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
                    }
                }
                else if (geometryObj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject in instanceGeometry)
                    {
                        if (geometryObject is Solid nestedSolid)
                        {
                            if (nestedSolid.Faces.Size > 0 && nestedSolid.Volume > 0)
                            {
                                foreach (Face face in nestedSolid.Faces)
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
                            }
                        }
                    }
                }
            }

            // for railing
            //start
            //if (cylindricalFaces.Count > 0)
            //{
            //    List<XYZ> listPoints = new List<XYZ>();
            //    foreach (var face in cylindricalFaces)
            //    {
            //        if (Are2VectorsParallel(face.Axis, XYZ.BasisZ))
            //        {
            //            XYZ firstOrigin = new XYZ();
            //            if (!face.Origin.IsAlmostEqualTo(firstOrigin))
            //            {
            //                listPoints.Add(face.Origin);
            //                firstOrigin = face.Origin;
            //            }
            //        }
            //    }

            //    listPoints.Sort((a, b) => listPoints.First().DistanceTo(a).CompareTo(listPoints.First().DistanceTo(b)));
            //    for (int i = 0; i < listPoints.Count - 1; i++)
            //    {
            //        CreateModelLineAutoPlane(doc, listPoints[i], listPoints[i + 1]);
            //    }
            //}

            //end

            return (planarFaces, cylindricalFaces);
        }
    }
}