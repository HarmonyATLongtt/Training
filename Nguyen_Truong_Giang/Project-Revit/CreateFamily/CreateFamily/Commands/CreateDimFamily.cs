using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace CreateFamily.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreateDimFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Reference selectedFamilyRef = uidoc.Selection.PickObject(ObjectType.Element, "Please select a column");
            FamilyInstance selectedElement = doc.GetElement(selectedFamilyRef.ElementId) as FamilyInstance;

            ReferenceArray referenceArray = new ReferenceArray();
            Options options = new Options();
            options.ComputeReferences = true;
            options.IncludeNonVisibleObjects = true;

            GeometryElement geometry = selectedElement.get_Geometry(options);
            if (geometry != null)
            {
                foreach (GeometryObject geomObj in geometry)
                {
                    if (geomObj is GeometryInstance geometryInstance)
                    {
                        GeometryElement symbolGeometry = selectedElement.HasModifiedGeometry()
                                                        ? geometryInstance.GetInstanceGeometry()
                                                        : geometryInstance.GetSymbolGeometry();
                        foreach (GeometryObject symbolGeomObj in symbolGeometry)
                        {
                            if (symbolGeomObj is Solid solid)
                                GetReferenceFromSolid(solid, XYZ.BasisY, ref referenceArray);
                        }
                    }
                    else if (geomObj is Solid solid)
                        GetReferenceFromSolid(solid, XYZ.BasisY, ref referenceArray);
                }
            }

            View activeView = uidoc.ActiveGraphicalView;
            DimensionType dimensionType = GetDimType(doc);
            if (dimensionType == null)
            {
                System.Windows.Forms.MessageBox.Show("linear dimension type not found");
                return Result.Cancelled;
            }

            using (Transaction transaction = new Transaction(doc, "Create Dimension"))
            {
                transaction.Start();

                XYZ p1 = uidoc.Selection.PickPoint();
                XYZ p2 = uidoc.Selection.PickPoint();

                XYZ start = new XYZ(p1.X, p1.Y, 0);
                XYZ end = new XYZ(p2.X, p2.Y, 0);

                Line line = Line.CreateBound(start, end);
                Dimension dim = doc.Create.NewDimension(activeView, line, referenceArray, dimensionType);
                Validate(referenceArray, selectedElement);

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        private void Validate(ReferenceArray refArr, Element elem)
        {
            bool isInstance = false;
            if (refArr != null && elem != null)
            {
                foreach (Reference reference in refArr)
                {
                    Element refElem = elem.Document.GetElement(reference);
                    if (refElem.Id == elem.Id)
                    {
                        isInstance = true;
                        System.Windows.Forms.MessageBox.Show("dim to instance");
                        break;
                    }
                }
            }
            if (!isInstance)
                System.Windows.Forms.MessageBox.Show("dim to symbol");
        }

        private void GetReferenceFromSolid(Solid solid, XYZ normal, ref ReferenceArray referenceArray)
        {
            if (solid != null
                && solid.Volume > 0
                && normal != null
                && !normal.IsAlmostEqualTo(XYZ.Zero))
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace pl && IsParallel(pl.FaceNormal, normal))
                        referenceArray.Append(face.Reference);
                }
            }
        }

        private DimensionType GetDimType(Document doc)
        {
            return new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfClass(typeof(DimensionType))
                    .Cast<DimensionType>()
                    .FirstOrDefault(x => x.StyleType == DimensionStyleType.Linear);
        }

        private bool IsParallel(XYZ vector1, XYZ vector2)
        {
            if (vector1.IsAlmostEqualTo(vector2) || vector1.IsAlmostEqualTo(vector2.Negate()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}