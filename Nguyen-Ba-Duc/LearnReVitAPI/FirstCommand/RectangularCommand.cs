using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System.Data;
using System.Text;
using System.IO;
using Autodesk.Revit.DB.Mechanical;
using System.Security.Cryptography;
using OpenQA.Selenium.BiDi.Modules.Input;
using Microsoft.SqlServer.Server;

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
                    Dictionary<Solid, List<Face>> dictSolid_Faces = GetFacesOnGeometry(element);
                    PrepareForCutSolid(dictSolid_Faces);
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

        private void PrepareForCutSolid(Dictionary<Solid, List<Face>> dictSolid_Faces)
        {
            foreach (var keyValue in dictSolid_Faces)
            {
                List<Face> facesIsRectangle = new List<Face>();
                List<Edge> listEdgesOfSolid = new List<Edge>();
                foreach (Face face in keyValue.Value)
                {
                    listEdgesOfSolid.AddRange(GetEgdesOfFace(face).ListEdges);
                    if (GetEgdesOfFace(face).Num == 4)
                    {
                        facesIsRectangle.Add(face);
                    }
                }
                var pairs = GetFaceToCreateRectangle(facesIsRectangle, listEdgesOfSolid);
                Plane plane = GetFaceToCutSolid(pairs.FacePairs, pairs.edge, pairs.ListFaces);
                if (plane != null)
                {
                    CutSolid(keyValue.Key, plane);
                }
            }
        }

        private void CutSolid(Solid solid, Plane plane)
        {
            Solid part = BooleanOperationsUtils.CutWithHalfSpace(solid, plane); // Phần trên LevelTop

            Plane flippedPlane = Plane.CreateByNormalAndOrigin(-plane.Normal, plane.Origin);
            Solid remaining = BooleanOperationsUtils.CutWithHalfSpace(solid, flippedPlane);

            if (IsSolidRetanglar(part))
            {
                // Create  new family
            }

            IList<Solid> separateSolids = SolidUtils.SplitVolumes(remaining);

            Dictionary<Solid, List<Face>> dictSolid_Faces = new Dictionary<Solid, List<Face>>();
            foreach (Solid s in separateSolids)
            {
                if (IsSolidRetanglar(s))
                {
                    // Create  new family
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
                        dictSolid_Faces.Add(solid, listFaces);
                    }
                }
            }
            PrepareForCutSolid(dictSolid_Faces);
        }

        private bool IsSolidRetanglar(Solid solid)
        {
            if (solid.Faces.Size == 6 && solid.Volume > 0)
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        if (GetEgdesOfFace(face).Num == 4)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private Plane GetFaceToCutSolid(List<Face> faces, Edge edge, List<Face> listFaces)
        {
            if (faces.Count == 2)
            {
                Face face = faces[0].Area > faces[1].Area ? faces[0] : faces[1];

                foreach (Face f in listFaces)
                {
                    foreach (Edge ed in GetEgdesOfFace(f).ListEdges)
                    {
                        Curve curve = ed.AsCurve();
                        if (ed != edge && IsIntersect(f, curve))
                        {
                            UV uv = new UV(0.5, 0.5);

                            XYZ normal = f.ComputeNormal(uv).Normalize();

                            XYZ p = f.Evaluate(uv);
                            XYZ reversed = -normal;

                            Plane plane = Plane.CreateByNormalAndOrigin(reversed, p);
                            return plane;
                        }
                    }
                }
            }
            return null;
        }

        private (List<Face> FacePairs, Edge edge, List<Face> ListFaces) GetFaceToCreateRectangle(List<Face> faces, List<Edge> listEdgesOfSolid)
        {
            Dictionary<Face, Face> facesParallel = new Dictionary<Face, Face>();
            Dictionary<Face, Face> facesVaLid = new Dictionary<Face, Face>();
            for (int i = 0; i < faces.Count - 1; i++)
            {
                for (int j = 1; j < faces.Count; j++)
                {
                    if (AreFacesParallel(faces[i], faces[j]))
                    {
                        facesParallel.Add(faces[i], faces[j]);
                    }
                }
            }

            Dictionary<List<Face>, Edge> distanceOfTwoFaces = new Dictionary<List<Face>, Edge>();

            foreach (var keyValue in FindEgde(listEdgesOfSolid, facesParallel))
            {
                List<Face> listFaces = new List<Face>();
                if (keyValue.Value.ApproximateLength < DistanceOfPointToFace(keyValue.Key[0], keyValue.Key[1]))
                {
                    listFaces.Add(keyValue.Key[0]);
                    listFaces.Add(keyValue.Key[1]);
                    distanceOfTwoFaces.Add(listFaces, keyValue.Value);
                }
            }
            var pairs = distanceOfTwoFaces.OrderByDescending(a => a.Value.ApproximateLength)
                    .First();
            return (pairs.Key, pairs.Value, faces);
        }

        private Dictionary<List<Face>, Edge> FindEgde(List<Edge> edges, Dictionary<Face, Face> facesParallel)
        {
            Dictionary<List<Face>, Edge> keyValuePairs = new Dictionary<List<Face>, Edge>();
            foreach (var keyValue in facesParallel)
            {
                List<Face> facePairs = new List<Face>();

                foreach (Edge edge in edges)
                {
                    Curve curve = edge.AsCurve();
                    {
                        if (IsLineVerticalToFace(curve, keyValue.Key) && IsIntersect(keyValue.Key, curve) && IsIntersect(keyValue.Value, curve))
                        {
                            facePairs.Add(keyValue.Key);
                            facePairs.Add(keyValue.Value);
                            keyValuePairs.Add(facePairs, edge);
                            break;
                        }
                    }
                }
            }
            return keyValuePairs;
        }

        private bool IsLineVerticalToFace(Curve curve, Face face, double tolerance = 1e-6)
        {
            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();

            UV uv = new UV(0.5, 0.5);
            XYZ faceNormal = face.ComputeNormal(uv).Normalize();

            double dot = lineDirection.DotProduct(faceNormal);

            return Math.Abs(dot) < tolerance;
        }

        private bool IsIntersect(Face face, Curve curve)
        {
            IntersectionResultArray results;
            SetComparisonResult comparisonResult = face.Intersect(curve, out results);

            if (comparisonResult == SetComparisonResult.Overlap && results != null)
            {
                return true;
            }
            return false;
        }

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

            XYZ p1 = face1.Evaluate(uv);
            XYZ p2 = face2.Evaluate(uv);

            double dot = normal1.DotProduct(normal2);

            return Math.Abs(dot + 1) < tolerance;
        }

        private (int Num, List<Edge> ListEdges) GetEgdesOfFace(Face face)
        {
            EdgeArrayArray edgeArrays = face.EdgeLoops;
            EdgeArray edges = edgeArrays.get_Item(0);

            List<Edge> edgeList = new List<Edge>();
            int num = 0;
            foreach (Edge edge in edges)
            {
                //Line line = edge.AsCurve() as Line;
                edgeList.Add(edge);
                num++;
            }
            return (num, edgeList);
        }

        private Dictionary<Solid, List<Face>> GetFacesOnGeometry(Element element)
        {
            Dictionary<Solid, List<Face>> keyValuePairs = new Dictionary<Solid, List<Face>>();

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            foreach (GeometryObject geometryObj in elementGeo)
            {
                List<Face> listFaces = new List<Face>();
                if (geometryObj is Solid solid)
                {
                    if (solid.Faces.Size > 0 && solid.Volume > 0)
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
                            if (nestedSolid.Faces.Size > 0 && nestedSolid.Volume > 0)
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