using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateDimension : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Lấy View hiện tại
            View view = doc.ActiveView;

            try
            {
                // Cho phép người dùng chọn hai đối tượng
                Reference ref1 = uidoc.Selection.PickObject(ObjectType.Element, "Select the first wall");
                Reference ref2 = uidoc.Selection.PickObject(ObjectType.Element, "Select the second wall");

                ElementId id1 = ref1.ElementId;
                ElementId id2 = ref2.ElementId;

                Element element1 = doc.GetElement(id1);
                Element element2 = doc.GetElement(id2);

                LocationCurve locCurve1 = element1.Location as LocationCurve;
                LocationCurve locCurve2 = element2.Location as LocationCurve;

                if (locCurve1 == null || locCurve2 == null)
                {
                    message = "One or both elements are not walls.";
                    return Result.Failed;
                }

                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Create Dimension");

                    // Lấy điểm bắt đầu và điểm kết thúc của các LocationCurve
                    XYZ point1 = locCurve1.Curve.GetEndPoint(0);
                    XYZ point2 = locCurve2.Curve.GetEndPoint(0);

                    // Kiểm tra khoảng cách giữa hai điểm
                    double distance = point1.DistanceTo(point2);
                    double shortCurveTolerance = doc.Application.ShortCurveTolerance;

                    if (distance < shortCurveTolerance)
                    {
                        message = $"The distance between points is too small: {distance} (ShortCurveTolerance: {shortCurveTolerance}).";
                        tx.RollBack();
                        return Result.Failed;
                    }

                    // Tạo một Line để làm cơ sở cho Dimension
                    Line line = Line.CreateBound(point1, point2);

                    // Tạo một đối tượng ReferenceArray để chứa các tham chiếu
                    ReferenceArray refArray = new ReferenceArray();
                    refArray.Append(new Reference(element1));
                    refArray.Append(new Reference(element2));

                    // Tạo Dimension
                    Dimension dimension = doc.Create.NewDimension(view, line, refArray);

                    tx.Commit();
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng hủy bỏ lựa chọn
                message = "Operation canceled by user.";
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateDimensionElementsWalls : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Select wall to dimension
            Reference pickRef = uiapp.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Select wall to dimension");
            Element selectedElem = doc.GetElement(pickRef);

            if (selectedElem is Wall)
            {
                Wall selectedWall = selectedElem as Wall;

                ReferenceArray referenceArray = new ReferenceArray();
                ReferenceArray referenceArray2 = new ReferenceArray();
                Reference r1 = null, r2 = null;

                Face wallFace = GetFace(selectedWall, selectedWall.Orientation);
                EdgeArrayArray edgeArrays = wallFace.EdgeLoops;
                EdgeArray edges = edgeArrays.get_Item(0);

                List<Edge> edgeList = new List<Edge>();
                foreach (Edge edge in edges)
                {
                    Line line = edge.AsCurve() as Line;

                    if (IsLineVertical(line) == true)
                    {
                        edgeList.Add(edge);
                    }
                }

                List<Edge> sortedEdges = edgeList.OrderByDescending(e => e.AsCurve().Length).ToList();
                r1 = sortedEdges[0].Reference;
                r2 = sortedEdges[1].Reference;

                referenceArray.Append(r1);

                // reference wall ends for overall dim
                referenceArray2.Append(r1);
                referenceArray2.Append(r2);

                List<BuiltInCategory> catList = new List<BuiltInCategory>() { BuiltInCategory.OST_Windows, BuiltInCategory.OST_Doors };
                ElementMulticategoryFilter wallFilter = new ElementMulticategoryFilter(catList);

                // get windows and doors from wall and create reference
                List<ElementId> wallElemsIds = selectedWall.GetDependentElements(wallFilter).ToList();

                foreach (ElementId elemId in wallElemsIds)
                {
                    FamilyInstance curFI = doc.GetElement(elemId) as FamilyInstance;
                    Reference curRef = GetSpecialFamilyReference(curFI, SpecialReferenceType.CenterLR);
                    //Reference curRef = GetSpecialFamilyReference(curFI, SpecialReferenceType.Left);
                    //Reference curRef2 = GetSpecialFamilyReference(curFI, SpecialReferenceType.Right);
                    referenceArray.Append(curRef);
                    //referenceArray.Append(curRef2);
                }

                referenceArray.Append(r2);

                // create dimension line
                LocationCurve wallLoc = selectedWall.Location as LocationCurve;
                Line wallLine = wallLoc.Curve as Line;

                XYZ offset1 = GetOffsetByWallOrientation(wallLine.GetEndPoint(0), selectedWall.Orientation, 5);
                XYZ offset2 = GetOffsetByWallOrientation(wallLine.GetEndPoint(1), selectedWall.Orientation, 5);

                XYZ offset1b = GetOffsetByWallOrientation(wallLine.GetEndPoint(0), selectedWall.Orientation, 10);
                XYZ offset2b = GetOffsetByWallOrientation(wallLine.GetEndPoint(1), selectedWall.Orientation, 10);

                Line dimLine = Line.CreateBound(offset1, offset2);
                Line dimLine2 = Line.CreateBound(offset1b, offset2b);

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Create new dimension");

                    Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, referenceArray);

                    if (wallElemsIds.Count > 0)
                    {
                        Dimension newDim2 = doc.Create.NewDimension(doc.ActiveView, dimLine2, referenceArray2);
                    }

                    t.Commit();
                }
            }
            else
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private XYZ GetOffsetByWallOrientation(XYZ point, XYZ orientation, int value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
        }

        public enum SpecialReferenceType
        {
            Left = 0,
            CenterLR = 1,
            Right = 2,
            Front = 3,
            CenterFB = 4,
            Back = 5,
            Bottom = 6,
            CenterElevation = 7,
            Top = 8
        }

        private Reference GetSpecialFamilyReference(FamilyInstance inst, SpecialReferenceType refType)
        {
            Reference indexRef = null;

            int idx = (int)refType;

            if (inst != null)
            {
                Document dbDoc = inst.Document;

                Options geomOptions = new Options();
                geomOptions.ComputeReferences = true;
                geomOptions.DetailLevel = ViewDetailLevel.Undefined;
                geomOptions.IncludeNonVisibleObjects = true;

                GeometryElement gElement = inst.get_Geometry(geomOptions);
                GeometryInstance gInst = gElement.First() as GeometryInstance;

                String sampleStableRef = null;

                if (gInst != null)
                {
                    GeometryElement gSymbol = gInst.GetSymbolGeometry();

                    if (gSymbol != null)
                    {
                        foreach (GeometryObject geomObj in gSymbol)
                        {
                            if (geomObj is Solid)
                            {
                                Solid solid = geomObj as Solid;

                                if (solid.Faces.Size > 0)
                                {
                                    Face face = solid.Faces.get_Item(0);
                                    sampleStableRef = face.Reference.ConvertToStableRepresentation(dbDoc);
                                    break;
                                }
                            }
                            else if (geomObj is Curve)
                            {
                                Curve curve = geomObj as Curve;
                                Reference curveRef = curve.Reference;
                                if (curveRef != null)
                                {
                                    sampleStableRef = curve.Reference.ConvertToStableRepresentation(dbDoc);
                                    break;
                                }
                            }
                            else if (geomObj is Point)
                            {
                                Point point = geomObj as Point;
                                sampleStableRef = point.Reference.ConvertToStableRepresentation(dbDoc);
                                break;
                            }
                        }
                    }

                    if (sampleStableRef != null)
                    {
                        String[] refTokens = sampleStableRef.Split(new char[] { ':' });

                        String customStableRef = refTokens[0] + ":"
                          + refTokens[1] + ":" + refTokens[2] + ":"
                          + refTokens[3] + ":" + idx.ToString();

                        indexRef = Reference.ParseFromStableRepresentation(dbDoc, customStableRef);

                        GeometryObject geoObj = inst.GetGeometryObjectFromReference(indexRef);

                        if (geoObj != null)
                        {
                            String finalToken = "";
                            if (geoObj is Edge)
                            {
                                finalToken = ":LINEAR";
                            }

                            if (geoObj is Face)
                            {
                                finalToken = ":SURFACE";
                            }

                            customStableRef += finalToken;
                            indexRef = Reference.ParseFromStableRepresentation(dbDoc, customStableRef);
                        }
                        else
                        {
                            indexRef = null;
                        }
                    }
                }
                else
                {
                    throw new Exception("No Symbol Geometry found...");
                }
            }
            return indexRef;
        }

        private bool IsLineVertical(Line line)
        {
            if (line.Direction.IsAlmostEqualTo(XYZ.BasisZ) || line.Direction.IsAlmostEqualTo(-XYZ.BasisZ))
                return true;
            else
                return false;
        }

        private Face GetFace(Element selectedElem, XYZ orientation)
        {
            PlanarFace returnFace = null;
            List<Solid> solids = GetSolids(selectedElem);

            foreach (Solid solid in solids)
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        PlanarFace pf = face as PlanarFace;

                        if (pf.FaceNormal.IsAlmostEqualTo(orientation))
                            returnFace = pf;
                    }
                }
            }

            return returnFace;
        }

        private List<Solid> GetSolids(Element selectedElem)
        {
            List<Solid> returnList = new List<Solid>();

            Options options = new Options();
            options.ComputeReferences = true;
            options.DetailLevel = ViewDetailLevel.Fine;

            GeometryElement geomElem = selectedElem.get_Geometry(options);

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        returnList.Add(solid);
                    }
                }
            }

            return returnList;
        }
    }
}