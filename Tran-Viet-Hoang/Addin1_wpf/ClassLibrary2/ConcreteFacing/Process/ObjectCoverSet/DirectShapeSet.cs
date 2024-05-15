using Autodesk.Revit.DB;
using ConcreteFacing.DATA;
using ConcreteFacing.UI.ViewModel;
using ConcreteFacing.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ConcreteFacing.Process.ObjectCoverSet
{
    public class DirectShapeSet
    {
        public void CreateDirectShapes(Document doc, MainView view, List<Element> elems)
        {
            int num = elems.Count();

            if (view.ShowDialog() == true)
            {
                using (var transaction = new Transaction(doc, "Set Cover"))
                {
                    transaction.Start();
                    foreach (var elem in elems)
                    {
                        FamilyInstance ins = elem as FamilyInstance;
                        MainViewModel vm = view.DataContext as MainViewModel;

                        double thickness = vm.SourceCatelv.First().Thickness / 304.8;

                        var Sol = elem.get_Geometry(new Options())
                            .OfType<Solid>()
                            .ToList();
                        if (Sol.Count() != 0)
                        {
                            //CreateDirectShape(doc, Sol, thickness, ins);
                            CreateSpecifyFaceCover(doc, Sol, vm, elem);
                        }
                        else
                        {
                            var InsGeometry = elem.get_Geometry(new Options())
                            .OfType<GeometryInstance>()
                            .SelectMany(x => x.GetInstanceGeometry().OfType<Solid>())
                            .ToList()
                            ;
                            //CreateDirectShape(doc, InsGeometry, thickness, ins);
                            CreateSpecifyFaceCover(doc, Sol, vm, elem);

                        }
                    }
                    MessageBox.Show("Đã apply covers cho " + num + " cấu kiện");
                    transaction.Commit();
                }
            }
            else
                MessageBox.Show("Command is cancel");
        }

        public void CreateDirectShape(Document doc, List<Solid> solids, double thickness, FamilyInstance ins)
        {
            var solid = solids.Where(i => i.Volume != 0).First();

            int x = new Random().Next(1, 6);
            XYZ topnorm = ins.HandOrientation.CrossProduct(ins.FacingOrientation);
            XYZ botnorm = topnorm.Negate();

            foreach (Face face in solid.Faces)
            {
                PlanarFace planarFace = face as PlanarFace;
                XYZ otherfacenorm = planarFace.FaceNormal;
                // cross là theo quy tắc bàn tay phải

                #region cmt

                int faceorder = -1;
                //top
                if (otherfacenorm.IsAlmostEqualTo(topnorm, 10E-5)) { faceorder = 1; };
                //bot
                if (otherfacenorm.IsAlmostEqualTo(botnorm, 10E-5)) { faceorder = 2; };
                //right
                if (otherfacenorm.IsAlmostEqualTo(ins.HandOrientation, 10E-5)) { faceorder = 3; };
                //left
                if (otherfacenorm.IsAlmostEqualTo(ins.HandOrientation.Negate(), 10E-5)) { faceorder = 4; };
                //front
                if (otherfacenorm.IsAlmostEqualTo(ins.FacingOrientation.Negate(), 10E-5)) { faceorder = 5; };
                //back
                if (otherfacenorm.IsAlmostEqualTo(ins.FacingOrientation, 10E-5)) { faceorder = 6; };

                #endregion cmt

                //bool top = otherfacenorm.IsAlmostEqualTo(topnorm, 10E-5);
                //bool bot = otherfacenorm.IsAlmostEqualTo(botnorm, 10E-5);
                //bool right = otherfacenorm.IsAlmostEqualTo(ins.HandOrientation, 10E-5);
                //bool left = otherfacenorm.IsAlmostEqualTo(ins.HandOrientation.Negate(), 10E-5);
                //bool front = otherfacenorm.IsAlmostEqualTo(ins.FacingOrientation.Negate(), 10E-5);
                //bool back = otherfacenorm.IsAlmostEqualTo(ins.FacingOrientation, 10E-5);

                if (faceorder == x)
                {
                    var eloop = face.GetEdgesAsCurveLoops();
                    Solid cover = GeometryCreationUtilities.CreateExtrusionGeometry(eloop, otherfacenorm, thickness);
                    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                    ds.SetShape(new GeometryObject[] { cover });
                }
            }
        }

        public void CreateSpecifyFaceCover(Document doc, List<Solid> solids, MainViewModel vm, Element elem)
        {
            var solid = solids.Where(i => i.Volume != 0).First();
            List<Face> faces = new List<Face>();
            foreach (Face face in solid.Faces)
            {
                faces.Add(face);
            }
           
            var sortedfaces = new UserOptions().Check(faces, vm, elem);
            foreach (var sortedface in sortedfaces.FaceIsCheck)
            {
                PlanarFace planarFace = sortedface as PlanarFace;

                var eloop = sortedface.GetEdgesAsCurveLoops();
                Solid cover = GeometryCreationUtilities.CreateExtrusionGeometry(eloop, planarFace.FaceNormal, sortedfaces.Thickness / 304.8);
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new GeometryObject[] { cover });
            }
        }
    }
}