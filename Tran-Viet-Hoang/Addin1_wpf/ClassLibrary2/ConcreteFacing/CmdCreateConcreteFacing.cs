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

            var elems = PickConcreteBeamOrColumn(uidoc);
            if (elems != null)
            {
                MainView view = new MainView();
                view.DataContext = new MainViewModel();
                int num = elems.Count();
                if (view.ShowDialog() == true)
                {
                    using (var transaction = new Transaction(doc, "Set Cover"))
                    {
                        transaction.Start();
                        foreach (var elem in elems)
                        {
                            FamilyInstance ins = elem as FamilyInstance;
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
                    MessageBox.Show("Command is cancel");
            }
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
                    int x = new Random().Next(1, 6);

                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace planarFace = face as PlanarFace;
                        XYZ otherfacenorm = planarFace.FaceNormal;
                        // cross là theo quy tắc bàn tay phải
                        XYZ topnorm = ins.HandOrientation.CrossProduct(ins.FacingOrientation);
                        XYZ botnorm = topnorm.Negate();

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
            }
        }
    }
}