using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;
using Microsoft.SqlServer.Server;
using static System.Net.Mime.MediaTypeNames;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateDimCommand : IExternalCommand
    {
        private double tolerance = 1e-9;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            ViewComboboxWindow viewWindow = new ViewComboboxWindow(doc);
            if (viewWindow.ShowDialog() == true)
            {
                double offset = viewWindow.Offset;

                Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick a wall");

                if (pickedRef != null)
                {
                    Element selectedElem = doc.GetElement(pickedRef);
                    if (selectedElem is Wall)
                    {
                        Wall selectedWall = selectedElem as Wall;

                        ElementId wallTypeId = selectedWall.GetTypeId();

                        XYZ wallOrientation = selectedWall.Orientation;

                        PrepareForDimmension(doc, selectedWall.Id, wallOrientation, offset);

                        return Result.Succeeded;
                    }
                    return Result.Failed;
                }
                return Result.Failed;
            }
            TaskDialog.Show("Note", "Element has been selected is not a wall");
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

        //________________
        //Start-- Add Dimmention

        private void PrepareForDimmension(Document doc, ElementId elementId, XYZ wallOrientation, double offset)
        {
            List<Edge> listEdges = new List<Edge>();
            List<Face> listFaces = new List<Face>();

            Wall wall = doc.GetElement(elementId) as Wall;
            LocationCurve wallLoc = wall.Location as LocationCurve;
            Line wallLine = wallLoc.Curve as Line;
            XYZ start = wallLine.GetEndPoint(0);
            XYZ end = wallLine.GetEndPoint(1);
            XYZ startOnZAxis = new XYZ(start.X, start.Y, 0);
            Face faceToRef = null;

            IList<Element> intersectElements = ListIntersectElements(doc, wall);
            listFaces = GetListFaces(intersectElements, wallLine);

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeometry = wall.get_Geometry(options);

            foreach (GeometryObject geometryObject in elementGeometry)
            {
                if (geometryObject is Solid solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (IsFaceNormal(face, wall.Orientation))
                        {
                            listEdges.AddRange(GetEdges(face));
                            faceToRef = face;
                            break;
                        }
                    }
                }
            }

            List<Edge> listEdgeToRef = GetListEdgeToRef(faceToRef, start, end);

            List<ReferenceInfo> listReferenceInfos = GetListReferenInfos(listEdgeToRef, listFaces, listEdges);

            listReferenceInfos.Sort((p1, p2) => startOnZAxis.DistanceTo(p1.Point).CompareTo(startOnZAxis.DistanceTo(p2.Point)));

            RunTransaction(doc, "Create Dimension", (Transaction t) =>
            {
                ReferenceArray referenceArray = new ReferenceArray();
                for (int i = 0; i < listReferenceInfos.Count() - 1; i++)
                {
                    CreateNewDim(doc, wallOrientation, referenceArray, listReferenceInfos[i], listReferenceInfos[i + 1], offset);
                    referenceArray.Clear();
                }
                CreateNewDim(doc, wallOrientation, referenceArray, listReferenceInfos[0], listReferenceInfos[listReferenceInfos.Count() - 1], offset + 5);
            });
        }

        private List<ReferenceInfo> GetListReferenInfos(List<Edge> listEdgeToRef, List<Face> listFaces, List<Edge> listEdges)
        {
            List<ReferenceInfo> listReferenceInfos = new List<ReferenceInfo>();
            foreach (Edge edge in listEdgeToRef)
            {
                foreach (Face face in listFaces)
                {
                    IntersectionResultArray results;
                    SetComparisonResult comparisonResult = face.Intersect(edge.AsCurve(), out results);

                    if (comparisonResult == SetComparisonResult.Overlap && results != null)
                    {
                        foreach (IntersectionResult result in results)
                        {
                            ReferenceInfo referenceInfo = new ReferenceInfo();
                            referenceInfo.Point = result.XYZPoint;
                            referenceInfo.Face = face;
                            listReferenceInfos.Add(referenceInfo);
                        }
                    }
                }
            }

            for (int i = 0; i < listEdges.Count(); i++)
            {
                ReferenceInfo referenceInfo = new ReferenceInfo();

                referenceInfo.Point = GetPointOnZAxis(listEdges[i], 0);

                referenceInfo.Edge = listEdges[i];

                listReferenceInfos.Add(referenceInfo);
            }
            return listReferenceInfos;
        }

        private List<Edge> GetListEdgeToRef(Face faceToRef, XYZ start, XYZ end)
        {
            List<Edge> listEdgeToRef = new List<Edge>();
            EdgeArrayArray edgeArrays = faceToRef.EdgeLoops;

            foreach (EdgeArray edges in edgeArrays)
            {
                foreach (Edge edge in edges)
                {
                    Line line = edge.AsCurve() as Line;

                    XYZ start1 = line.GetEndPoint(0);
                    XYZ end1 = line.GetEndPoint(1);

                    if (start1.Z == start.Z && end1.Z == end.Z)
                    {
                        listEdgeToRef.Add(edge);
                    }
                }
            }
            return listEdgeToRef;
        }

        private List<Face> GetListFaces(IList<Element> intersectElements, Line wallLine)
        {
            List<Face> listFaces = new List<Face>();
            foreach (Element element in intersectElements)
            {
                Options optionsEle = new Options();
                optionsEle.DetailLevel = ViewDetailLevel.Fine;
                optionsEle.ComputeReferences = true;
                GeometryElement elementGeo = element.get_Geometry(optionsEle);

                foreach (GeometryObject geometryObj in elementGeo)
                {
                    if (geometryObj is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (IsFaceNormalWithLine(face, wallLine))
                            {
                                listFaces.Add(face);
                            }
                        }
                    }
                }
            }
            return listFaces;
        }

        private IList<Element> ListIntersectElements(Document doc, Element wall)
        {
            ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementCategoryFilter columnFilter = new ElementCategoryFilter(BuiltInCategory.OST_Columns);
            LogicalOrFilter orFilter = new LogicalOrFilter(wallFilter, columnFilter);
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(wall);

            IList<Element> listElements = new FilteredElementCollector(doc)
                .WherePasses(orFilter)
                .WherePasses(filter)
                .ToElements();

            return listElements;
        }

        private void CreateNewDim(Document doc, XYZ wallOrientation, ReferenceArray referenceArray, ReferenceInfo referenceInfo1, ReferenceInfo referenceInfo2, double offset)
        {
            XYZ point1 = new XYZ(0, 0, 0);
            XYZ point2 = new XYZ(0, 0, 0);
            XYZ vectorZ = new XYZ(0, 0, -1);
            Reference r1 = null;
            Reference r2 = null;

            if (doc.ActiveView.Name == "Level 1")
            {
                point1 = GetOffsetByWallOrientation(referenceInfo1.Point, wallOrientation, offset);
                point2 = GetOffsetByWallOrientation(referenceInfo2.Point, wallOrientation, offset);
            }
            else if (doc.ActiveView.Name == "North")
            {
                point1 = GetOffsetByWallOrientation(referenceInfo1.Point, vectorZ, offset);
                point2 = GetOffsetByWallOrientation(referenceInfo2.Point, vectorZ, offset);
            }

            if (referenceInfo1.Face != null)
            {
                r1 = referenceInfo1.Face.Reference;
            }
            else if (referenceInfo1.Edge != null)
            {
                r1 = referenceInfo1.Edge.Reference;
            }

            if (referenceInfo2.Face != null)
            {
                r2 = referenceInfo2.Face.Reference;
            }
            else if (referenceInfo2.Edge != null)
            {
                r2 = referenceInfo2.Edge.Reference;
            }

            referenceArray.Append(r1);
            referenceArray.Append(r2);

            Line dimLine = Line.CreateBound(point1, point2);

            Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, referenceArray);
        }

        private XYZ GetOffsetByWallOrientation(XYZ point, XYZ orientation, double value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
        }

        private List<Edge> GetEdges(Face face)
        {
            List<Edge> edgeList = new List<Edge>();

            EdgeArrayArray edgeArrays = face.EdgeLoops;

            foreach (EdgeArray edges in edgeArrays)
            {
                foreach (Edge edge in edges)
                {
                    Line line = edge.AsCurve() as Line;

                    if (IsLineVertical(line))
                    {
                        edgeList.Add(edge);
                    }
                }
            }
            return edgeList;
        }

        private XYZ GetPointOnZAxis(Edge edge, double height)
        {
            Line line = edge.AsCurve() as Line;
            XYZ start = line.GetEndPoint(0);
            XYZ newPointOnZAxis = new XYZ(start.X, start.Y, height);
            return newPointOnZAxis;
        }

        private bool IsFaceNormal(Face face, XYZ orientaion)
        {
            if (face is PlanarFace)
            {
                PlanarFace pf = face as PlanarFace;
                if (pf.FaceNormal.Normalize().IsAlmostEqualTo(orientaion, tolerance))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsFaceNormalWithLine(Face face, Line line)
        {
            XYZ lineDirection = line.Direction;
            if (face is PlanarFace)
            {
                PlanarFace pf = face as PlanarFace;
                return pf.FaceNormal.Normalize().IsAlmostEqualTo(lineDirection.Normalize(), tolerance) ||
                    pf.FaceNormal.Normalize().IsAlmostEqualTo(-lineDirection.Normalize(), tolerance);
            }

            return false;
        }

        private bool IsLineVertical(Line line)
        {
            if (line.Direction.IsAlmostEqualTo(XYZ.BasisZ) || line.Direction.IsAlmostEqualTo(-XYZ.BasisZ))
                return true;
            else
                return false;
        }

        //End________________
    }

    public class ReferenceInfo
    {
        public XYZ Point { get; set; }
        public Edge Edge { get; set; }
        public Face Face { get; set; }
    }
}