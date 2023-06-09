using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

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



            Reference selectedFamilyRef = uidoc.Selection.PickObject(ObjectType.Element, "Please select a family");
            Element selectedElement = doc.GetElement(selectedFamilyRef.ElementId);

            ReferenceArray referenceArray = new ReferenceArray();
            Options options = new Options();
            options.ComputeReferences = true;
            options.IncludeNonVisibleObjects = true;
            options.View = uidoc.ActiveGraphicalView;
            
            GeometryElement geometry = selectedElement.get_Geometry(options);
            if (geometry != null)
            {
                foreach (GeometryObject geomObj in geometry)
                {
                    if (geomObj is GeometryInstance geometryInstance)
                    {
                        GeometryElement symbolGeometry = geometryInstance.GetInstanceGeometry();
                        foreach (GeometryObject symbolGeomObj in symbolGeometry)
                        {
                            if (symbolGeomObj is Solid solid)
                            {
                                foreach (Face face in solid.Faces)
                                {
                                    if (face is PlanarFace pl && IsParallel(pl.FaceNormal, XYZ.BasisY))                            
                                           referenceArray.Append(face.Reference);                                    
                                      
                                }
                            }
                        }
                    }
                    else if (geomObj is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace pl && IsParallel(pl.FaceNormal, XYZ.BasisY))
                                referenceArray.Append(face.Reference);
                        }
                    }
                }
            }

            View activeView = uidoc.ActiveGraphicalView;

            using (Transaction transaction = new Transaction(doc, "Create Dimension"))
            {
                transaction.Start();

                //  XYZ startPoint = uidoc.Selection.PickPoint("Please select a point to create dimension");

                // Line line = Line.CreateBound(startPoint, selectedFamilyRef.GlobalPoint);
                XYZ p1 = uidoc.Selection.PickPoint();
                XYZ p2 = uidoc.Selection.PickPoint();

                //ReferenceArray arere = new ReferenceArray();
                //arere.Append(uidoc.Selection.PickObject(ObjectType.Face));
                //arere.Append(uidoc.Selection.PickObject(ObjectType.Face));

                Line line = Line.CreateBound(new XYZ(p1.X, p1.Y, activeView.GenLevel.Elevation), new XYZ(p2.X, p2.Y, activeView.GenLevel.Elevation));
                Dimension dim = doc.Create.NewDimension(activeView, line, referenceArray);

                transaction.Commit();
            }

            return Result.Succeeded;
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