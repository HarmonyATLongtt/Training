using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bai4.View;
using Bai4.ViewModel;
using Bai4.Model;
using System.Windows.Forms;

namespace Bai4
{
    [Transaction(TransactionMode.Manual)]
    public class CreateGeometryCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document doc = uIDocument.Document;
            try
            {
                MainModel mainModel = new MainModel(doc);
                MainViewModel mainViewModel = new MainViewModel(mainModel);
                CreateGeometryView mainview = new CreateGeometryView();
                mainview.DataContext = mainViewModel;
                
                IList<XYZ> ListIntersection = ListIntersectionGrid(doc);

                if (ListIntersection.Count < 1)
                {
                    MessageBox.Show("Chưa có giao điểm grid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (ListIntersection.Count >= 1)
                {   
                    if(mainview.ShowDialog() == true)
                    {
                        if (mainViewModel.CubeSelect == false && mainViewModel.SphereSelect == false)
                        {
                            MessageBox.Show("Chưa chọn tạo hình lập phương hoặc hình cầu", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (mainViewModel.CubeSelect && mainViewModel.CubeLength > 0)
                        {
                            CreatCube(doc, mainModel.CubeLength, ListIntersection);
                        }
                        if (mainViewModel.SphereSelect && mainViewModel.SphereRadius > 0)
                        {
                            CreateSphere(doc, mainModel.SphereRadius, ListIntersection);
                        }
                    }
                }  
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
        /// <summary>
        /// creat and place Sphere DirectShape at the intersections
        /// <param name="doc"></param>
        /// <param name="radius"></param>
        /// <param name="intersection"></param>
        public void CreateSphere(Document doc, double radius, IList<XYZ> intersection)
        {
            if (doc != null && intersection.Count > 0 && radius > 0)
            {
                foreach (var x in intersection)
                {
                    List<Curve> profile = new List<Curve>();
                    XYZ center = x;
                    XYZ profile00 = center;
                    XYZ profilePlus = center + new XYZ(0, radius, 0);
                    XYZ profileMinus = center - new XYZ(0, radius, 0);

                    profile.Add(Line.CreateBound(profilePlus, profileMinus));
                    profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

                    CurveLoop curveLoop = CurveLoop.Create(profile);
                    SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

                    Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
                    if (Frame.CanDefineRevitGeometry(frame) == true)
                    {
                        Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                        using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
                        {
                            t.Start();
                            // create direct shape and assign the sphere shape
                            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                            ds.ApplicationId = "Application id";
                            ds.ApplicationDataId = "Geometry object id";
                            ds.SetShape(new GeometryObject[] { sphere });
                            t.Commit();
                        }
                    }
                }
            }
        }
        /// <summary>
        ///  creat and place Cube DirectShape at the intersections 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="d"></param>
        /// <param name="intersection"></param>
        public void CreatCube(Document doc, double d, IList<XYZ> intersection)
        {
            if (doc != null && intersection.Count > 0 && d > 0)
            {
                foreach (var x in intersection)
                {
                    List<Curve> profile = new List<Curve>();
                    XYZ center = x;
                    XYZ profile00 = x + new XYZ(d / 2, -d / 2, -d / 2);
                    XYZ profile01 = x + new XYZ(d / 2, d / 2, -d / 2);
                    XYZ profile11 = x + new XYZ(-d / 2, d / 2, -d / 2);
                    XYZ profile10 = x + new XYZ(-d / 2, -d / 2, -d / 2);

                    profile.Add(Line.CreateBound(profile00, profile01));
                    profile.Add(Line.CreateBound(profile01, profile11));
                    profile.Add(Line.CreateBound(profile11, profile10));
                    profile.Add(Line.CreateBound(profile10, profile00));

                    CurveLoop curveLoop = CurveLoop.Create(profile);

                    SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

                    Solid cube = GeometryCreationUtilities.CreateExtrusionGeometry(new CurveLoop[] { curveLoop }, XYZ.BasisZ, d, options);
                    using (Transaction t = new Transaction(doc, "Create cube direct shape"))
                    {
                        t.Start();

                        DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                        ds.SetName("Cube");
                        ds.SetShape(new GeometryObject[] { cube });

                        t.Commit();
                    }
                }
            }
        }
        /// <summary>
        /// extract a list Line 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private IList<Line> GetIntersectionGrid(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                     .OfCategory(BuiltInCategory.OST_Grids)
                                                     .OfClass(typeof(Grid))
                                                     .Cast<Grid>()
                                                     .Where(x => x.IsCurved == false)
                                                     .Select(x => x.Curve)
                                                     .Cast<Line>()
                                                     .ToList();
        }
        /// <summary>
        /// get a list of intersection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public IList<XYZ> ListIntersectionGrid(Document doc)
        {
            if (GetIntersectionGrid(doc).Count > 0 && doc != null)
            {
                List<XYZ> IntersectionList = new List<XYZ>();
                foreach (var curve1 in GetIntersectionGrid(doc))
                {
                    foreach (var curve2 in GetIntersectionGrid(doc))
                    {
                        if (curve2 != curve1)
                        {
                            if ((curve2.Intersect(curve1, out IntersectionResultArray resultArray)) != SetComparisonResult.Equal)
                            {
                                if (resultArray != null)
                                {
                                    for (int i = 0; i < resultArray.Size; i++)
                                    {
                                        IntersectionList.Add(resultArray.get_Item(i).XYZPoint);
                                    }
                                }
                            }
                        }
                    }
                }
                return IntersectionList.Distinct(new FilterIntersection()).ToList(); ;
            }
            else
            {
                return new List<XYZ>();
            }
        }
        /// <summary>
        /// compare two object in certain conditions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public class FilterIntersection : IEqualityComparer<XYZ>
        {
            public bool Equals(XYZ x, XYZ y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.IsAlmostEqualTo(y);
            }
            public int GetHashCode(XYZ obj)
            {
                return 1;
            }
        }
    }
}
