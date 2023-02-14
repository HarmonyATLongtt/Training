using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ConcreteFacing.UI.ViewModel;
using ConcreteFacing.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ConcreteFacing
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CmdCreateConcreteFacing : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            MainView view = new MainView();
            view.DataContext = new MainViewModel();

            var elems = PickConcreteBeamOrColumn(uidoc);
            //var elems = SelectedConcreteBeamOrColumn(uidoc);
            int num = elems.Count();

            if (elems != null && view.ShowDialog() == false)
            {
                using (var transaction = new Transaction(doc, "Set Mark"))
                {
                    transaction.Start();
                    foreach (var elem in elems)
                    {
                        FamilyInstance ins = elem as FamilyInstance;
                        BoundingBoxXYZ boundingbox = ins.get_BoundingBox(null);
                        string info = "Cấu kiện " + ins.Name + " có:" + "\n" +
                           "Vị trí Min BoundingBox là: " + boundingbox.Min.ToString() + "\n" +
                           "Vị trí Max BoundingBox là: " + boundingbox.Max.ToString();

                        double thickness = 20 / 304.8;

                        var Sol = elem.get_Geometry(new Options())
                            .OfType<Solid>()
                            .ToList();
                        if (Sol.Count() != 0)
                        {
                            CreateDirectShape(doc, Sol, thickness, ins);
                        }
                        else
                        {
                            var InsGeometry = elem.get_Geometry(new Options())
                            .OfType<GeometryInstance>()
                            .SelectMany(x => x.GetInstanceGeometry().OfType<Solid>())
                            .ToList()
                            ;
                            CreateDirectShape(doc, InsGeometry, thickness, ins);
                        }
                    }
                    MessageBox.Show("Đã apply covers cho " + num + " cấu kiện");
                    transaction.Commit();
                }
            }
            else
                MessageBox.Show("nguoi dung huy thao tac");

            return Result.Succeeded;
        }

        private List<Element> PickConcreteBeamOrColumn(UIDocument uidoc)
        {
            try
            {
                List<Element> result = new List<Element>();
                IList<Reference> refer = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new Common.AllowBeamAndColumn(), "chon dam hoac cot be tong") as IList<Reference>;
                if (refer != null)

                    foreach (Reference r in refer)
                    {
                        result.Add(uidoc.Document.GetElement(r));
                    }
                return result;
            }
            catch (OperationCanceledException)
            {
            }
            return null;
        }

        private List<Element> SelectedConcreteBeamOrColumn(UIDocument uidoc)
        {
            try
            {
                ICollection<ElementId> result = new List<ElementId>();
                List<Element> allelem = uidoc.Selection.GetElementIds().Select(id => uidoc.Document.GetElement(id)).ToList();
                List<Element> beamandcol = new List<Element>();
                foreach (Element ele in allelem)
                {
                    FamilyInstance ins = ele as FamilyInstance;
                    if (ins != null && (ins.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column ||
                        ins.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam))
                    {
                        beamandcol.Add(ele);
                        result.Add(ele.Id);
                    }
                }

                return beamandcol;
            }
            catch (OperationCanceledException)
            {
            }
            return null;
        }

        public void CreateDirectShape(Document doc, List<Solid> solids, double thickness, FamilyInstance ins)
        {
            foreach (var solid in solids)
            {
                var faces = solid.Faces.Size;
                if (solid.Volume != 0)
                {
                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace planarFace = face as PlanarFace;
                        XYZ vecX = planarFace.XVector.Negate();
                        XYZ vecY = planarFace.YVector.Negate();
                        XYZ otherfacenorm = planarFace.FaceNormal;
                        // cross là theo quy tắc bàn tay phải
                        XYZ topnorm = ins.HandOrientation.CrossProduct(ins.FacingOrientation);
                        XYZ botnorm = topnorm.Negate();

                        bool top = otherfacenorm.IsAlmostEqualTo(topnorm, 10E-5);
                        bool bot = otherfacenorm.IsAlmostEqualTo(botnorm, 10E-5);
                        bool right = otherfacenorm.IsAlmostEqualTo(ins.HandOrientation, 10E-5);
                        bool left = otherfacenorm.IsAlmostEqualTo(ins.HandOrientation.Negate(), 10E-5);
                        bool front = otherfacenorm.IsAlmostEqualTo(ins.FacingOrientation.Negate(), 10E-5);
                        bool back = otherfacenorm.IsAlmostEqualTo(ins.FacingOrientation, 10E-5);

                        if (left == true)
                        {
                            var eloop = face.GetEdgesAsCurveLoops();
                            Solid cover = GeometryCreationUtilities.CreateExtrusionGeometry(eloop, otherfacenorm, thickness);
                            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                            ds.SetShape(new GeometryObject[] { cover });
                        }
                    }
                }
            }
        }
    }
}