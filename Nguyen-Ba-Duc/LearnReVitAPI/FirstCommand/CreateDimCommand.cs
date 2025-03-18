using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;
using Microsoft.SqlServer.Server;

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
                double sectionDepth = viewWindow.SectionDepth;

                Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick a wall");

                if (pickedRef != null)
                {
                    Element selectedElem = doc.GetElement(pickedRef);
                    if (selectedElem is Wall)
                    {
                        Wall selectedWall = selectedElem as Wall;

                        List<FamilyInstance> familyInstances = GetInstanceOnWall(doc, selectedWall);
                        CreateSectionView(doc, familyInstances, selectedWall, sectionDepth);

                        var views = new FilteredElementCollector(doc)
                              .OfClass(typeof(Autodesk.Revit.DB.View))
                              .OfType<Autodesk.Revit.DB.View>()
                              .Where(v => !v.IsTemplate)
                              .ToList();
                        foreach (var view in views)
                        {
                            if (view.ViewType == ViewType.Section)
                            {
                                FamilyInstance intance = GetInstanceOnSection(doc, view.Id, selectedWall);
                                PrepareDimForSectionView(intance, selectedWall, doc, view, offset);
                            }
                            else
                            {
                                PrepareForDimmension(doc, selectedWall.Id, selectedWall.Orientation, offset, view);
                            }
                        }

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

        private void PrepareDimForSectionView(FamilyInstance instance, Wall selectedWall, Document doc, Autodesk.Revit.DB.View view, double offset)
        {
            BoundingBoxXYZ instanceBbox = instance.get_BoundingBox(null);
            XYZ midPoint = (instanceBbox.Min + instanceBbox.Max) / 2;

            List<Face> listFaces = new List<Face>();
            List<Face> listFacesToChoose = new List<Face>();

            foreach (Face face in GetFacesOnGeometry(selectedWall))
            {
                if (IsFaceNormalEqualToZ(face))
                {
                    listFaces.Add(face);
                }
            }

            listFaces.Sort((a, b) => GetMidPointOfFace(a).Z.CompareTo(GetMidPointOfFace(b).Z));
            listFacesToChoose.Add(listFaces.First());
            listFacesToChoose.Add(listFaces.Last());
            foreach (Face face in listFaces)
            {
                if (IsInBoundingBox(GetMidPointOfFace(face), instanceBbox))
                {
                    listFacesToChoose.Add(face);
                }
            }
            listFacesToChoose.Sort((a, b) => GetMidPointOfFace(a).Z.CompareTo(GetMidPointOfFace(b).Z));

            RunTransToCreateDim(doc, listFacesToChoose, view, offset, midPoint, view.UpDirection, view.RightDirection);
        }

        private void RunTransToCreateDim(Document doc, List<Face> listFaces, Autodesk.Revit.DB.View view, double offset, XYZ point, XYZ vector, XYZ direction)
        {
            RunTransaction(doc, "Create Dimension", (Transaction t) =>
            {
                ReferenceArray referenceArray = new ReferenceArray();
                for (int i = 0; i < listFaces.Count() - 1; i++)
                {
                    CreateDim(GetOffset(point, direction, offset), doc, vector, referenceArray, listFaces[i], listFaces[i + 1], view);
                    referenceArray.Clear();
                }
                CreateDim(GetOffset(point, direction, offset + 5), doc, vector, referenceArray, listFaces[0], listFaces[listFaces.Count() - 1], view);
            });
        }

        private void CreateDim(XYZ point, Document doc, XYZ vector, ReferenceArray referenceArray, Face face1, Face face2, Autodesk.Revit.DB.View view)
        {
            Reference r1 = null;
            Reference r2 = null;

            r1 = face1.Reference;
            r2 = face2.Reference;

            referenceArray.Append(r1);
            referenceArray.Append(r2);

            Line dimLine = Line.CreateUnbound(point, vector);

            Dimension newDim = doc.Create.NewDimension(view, dimLine, referenceArray);
        }

        private XYZ GetMidPointOfFace(Face face)
        {
            BoundingBoxUV bbox = face.GetBoundingBox();
            UV midUV = new UV((bbox.Min.U + bbox.Max.U) / 2, (bbox.Min.V + bbox.Max.V) / 2);
            return face.Evaluate(midUV);
        }

        private bool IsInBoundingBox(XYZ point, BoundingBoxXYZ bbox)
        {
            XYZ bboxMin = bbox.Min;
            XYZ bboxMax = bbox.Max;
            bool result = false;

            if (point.X >= bboxMin.X && point.X <= bboxMax.X &&
                point.Y >= bboxMin.Y && point.Y <= bboxMax.Y &&
                point.Z >= bboxMin.Z && point.Z <= bboxMax.Z)
            {
                result = true;
            }

            return result;
        }

        private bool IsFaceNormalEqualToZ(Face face)
        {
            if (face is PlanarFace)
            {
                PlanarFace pf = face as PlanarFace;

                if (pf.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ, tolerance) || pf.FaceNormal.IsAlmostEqualTo(-XYZ.BasisZ, tolerance))
                {
                    return true;
                }
            }
            return false;
        }

        private FamilyInstance GetInstanceOnSection(Document doc, ElementId viewId, Wall selectedWall)
        {
            List<FamilyInstance> familyInstances = new FilteredElementCollector(doc, viewId)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();
            FamilyInstance familyInstance = null;
            foreach (FamilyInstance instance in familyInstances)
            {
                if (instance.Host != null && instance.Host.Id == selectedWall.Id)
                {
                    familyInstance = instance;
                    break;
                }
            }
            return familyInstance;
        }

        private void CreateSectionView(Document doc, List<FamilyInstance> familyInstances, Wall wall, double sectionDepth)
        {
            foreach (FamilyInstance familyInstance in familyInstances)
            {
                BoundingBoxXYZ boundingBox = familyInstance.get_BoundingBox(null);

                XYZ familyCenter = (boundingBox.Max + boundingBox.Min) / 2;
                XYZ wallOrientation = wall.Orientation;

                Transform sectionTransform = Transform.Identity;
                sectionTransform.Origin = familyCenter;
                sectionTransform.BasisX = wallOrientation;
                sectionTransform.BasisY = XYZ.BasisZ;
                sectionTransform.BasisZ = wallOrientation.CrossProduct(XYZ.BasisZ);

                // Kích thước mặt cắt
                double sectionWidth = 25;   // Chiều rộng
                //double sectionDepth = 6; // Độ sâu
                double sectionHeight = 35;  // Chiều cao

                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
                sectionBox.Transform = sectionTransform;
                sectionBox.Min = new XYZ(-sectionWidth / 2, -sectionHeight / 2, 0);
                sectionBox.Max = new XYZ(sectionWidth / 2, sectionHeight / 2, sectionDepth / 2);

                ViewFamilyType sectionType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);
                if (sectionType == null)
                {
                    TaskDialog.Show("Lỗi", "Không tìm thấy ViewFamilyType cho Section.");
                }
                RunTransaction(doc, "Create ViewSection", (Transaction t) =>
                {
                    ViewSection sectionView = ViewSection.CreateSection(doc, sectionType.Id, sectionBox);
                });
            }
        }

        private void PrepareForDimmension(Document doc, ElementId elementId, XYZ wallOrientation, double offset, Autodesk.Revit.DB.View view)
        {
            List<Face> listFaces = new List<Face>();

            Wall wall = doc.GetElement(elementId) as Wall;
            LocationCurve wallLoc = wall.Location as LocationCurve;
            Line wallLine = wallLoc.Curve as Line;
            XYZ start = wallLine.GetEndPoint(0);

            IList<Element> intersectElements = ListIntersectElements(doc, wall);
            listFaces = GetListFaces(intersectElements, wallLine);

            foreach (Face face in GetFacesOnGeometry(wall))
            {
                if (IsFaceNormalWithLine(face, wallLine))
                {
                    listFaces.Add(face);
                }
            }

            listFaces.Sort((a, b) => GetMidPointOfFace(a).X.CompareTo(GetMidPointOfFace(b).X));

            if (view.ViewType == ViewType.FloorPlan)
            {
                RunTransToCreateDim(doc, listFaces, view, offset, start, wallLine.Direction.Normalize(), wallOrientation);
            }
            else if (view.ViewType == ViewType.Elevation)

            {
                RunTransToCreateDim(doc, listFaces, view, offset, start, view.RightDirection, -XYZ.BasisZ);
            }
        }

        private List<Face> GetListFaces(IList<Element> intersectElements, Line wallLine)
        {
            List<Face> listFaces = new List<Face>();
            foreach (Element element in intersectElements)
            {
                foreach (Face face in GetFacesOnGeometry(element))
                {
                    if (IsFaceNormalWithLine(face, wallLine))
                    {
                        listFaces.Add(face);
                    }
                }
            }
            return listFaces;
        }

        private List<Face> GetFacesOnGeometry(Element element)
        {
            List<Face> listFaces = new List<Face>();
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            foreach (GeometryObject geometryObj in elementGeo)
            {
                if (geometryObj is Solid solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (face is PlanarFace)
                        {
                            listFaces.Add(face);
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

        private XYZ GetOffset(XYZ point, XYZ orientation, double value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
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

        private List<FamilyInstance> GetInstanceOnWall(Document doc, Wall wall)
        {
            ElementCategoryFilter windowFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            ElementCategoryFilter doorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            LogicalOrFilter orFilter = new LogicalOrFilter(windowFilter, doorFilter);

            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WherePasses(orFilter)
                .OfClass(typeof(FamilyInstance));

            List<FamilyInstance> listFamilies = new List<FamilyInstance>();
            foreach (FamilyInstance instance in collector)
            {
                if (instance.Host != null && instance.Host.Id == wall.Id)
                {
                    listFamilies.Add(instance);
                }
            }
            return listFamilies;
        }

        //End________________
    }
}