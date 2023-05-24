using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ExRevitAPI.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ExRevitAPI.ModelView
{
    internal class MainModelView : BaseModelView
    {
        private SelectModel _model;
        private List<string> _selectShape;
        private double _dimension;

        private string _selecShapeValue;

        public int TheSelectedIndex { get; set; }

        public List<string> SelectShapre
        {
            get => _selectShape;
            set
            {
                _selectShape = value;
                OnPropertyChanged(nameof(SelectShapre));
            }
        }

        public string SelecShapeValue
        {
            get => _selecShapeValue;
            set
            {
                _selecShapeValue = value;
                OnPropertyChanged(nameof(SelecShapeValue));
            }
        }

        public double Dimension
        {
            get => _dimension;
            set
            {
                _dimension = value;
                OnPropertyChanged(nameof(Dimension));
            }
        }

        public ICommand CreateGeometryCommand { get; set; }

        public MainModelView(SelectModel model)
        {
            SelectShapre = new List<string> { "Cube", "Sphere" };

            _model = model;
            CreateGeometryCommand = new RelayCommand<object>(CreateGeometry);
        }

        private void CreateGeometry(object parameter)
        {
            if (SelecShapeValue == "Cube")
            {
                CreateCube(_model.Doc, Dimension);
            }
            else if (SelecShapeValue == "Sphere")
            {
                CreateSphere(_model.Doc, Dimension);
            }
        }

        private void CreateGird(Document doc)
        {
            // Tạo hai điểm đại diện cho đường Grid
            XYZ pointA = new XYZ(0, 0, 0);
            XYZ pointB = new XYZ(50, 0, 0);

            XYZ pointC = new XYZ(25, -25, 0);
            XYZ pointD = new XYZ(25, 25, 0);

            // Tạo đường Grid từ hai điểm trên
            Line line1 = Line.CreateBound(pointA, pointB);
            Line line2 = Line.CreateBound(pointC, pointD);

            // Tạo Grid từ đường Grid
            using (Transaction trans = new Transaction(doc, "Create Grids"))
            {
                trans.Start();
                Grid grid1 = Grid.Create(doc, line1);
                Grid grid2 = Grid.Create(doc, line2);

                IntersectionResultArray intersectionResults = new IntersectionResultArray();

                SetComparisonResult result = line1.Intersect(line2, out intersectionResults);

                // Đặt tên cho hai Grid
                grid1.Name = "Grid1";
                grid2.Name = "Grid2";

                trans.Commit();
            }
        }

        private void CreateCube(Document doc, double dimension)
        {
            List<Curve> profile = new List<Curve>();
            dimension = Dimension;

            // Tạo các điểm đại diện cho các đỉnh của hình lập phương
            XYZ pointA = XYZ.Zero;
            XYZ pointB = new XYZ(dimension, 0, 0);
            XYZ pointC = new XYZ(dimension, dimension, 0);
            XYZ pointD = new XYZ(0, dimension, 0);

            // Tạo các đoạn thẳng để tạo hình lập phương
            Line lineAB = Line.CreateBound(pointA, pointB);
            Line lineBC = Line.CreateBound(pointB, pointC);
            Line lineCD = Line.CreateBound(pointC, pointD);
            Line lineDA = Line.CreateBound(pointD, pointA);

            // Thêm các đoạn thẳng vào CurveLoop
            CurveLoop curveLoop = new CurveLoop();
            curveLoop.Append(lineAB);
            curveLoop.Append(lineBC);
            curveLoop.Append(lineCD);
            curveLoop.Append(lineDA);

            // Tạo SolidOptions để chỉ định các tùy chọn cho việc tạo hình lập phương.
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            // Tạo Frame để xác định vị trí và hướng của hình lập phương.

            Frame frame = new Frame(GetCreatePoint(doc), XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame) == true) // Kiểm tra xem Frame có thể định nghĩa hình học trong Revit hay không.
            {
                // Tính toán vector dịch chuyển từ điểm ban đầu của CurveLoop đến điểm mới
                XYZ translationVector = GetCreatePoint(doc);

                // Di chuyển đoạn thẳng trong CurveLoop đến điểm mới
                CurveLoop translatedCurveLoop = new CurveLoop();

                foreach (Curve curve in curveLoop)
                {
                    XYZ startPoint = curve.GetEndPoint(0) + translationVector;
                    XYZ endPoint = curve.GetEndPoint(1) + translationVector;

                    Line translatedLine = Line.CreateBound(startPoint, endPoint);
                    translatedCurveLoop.Append(translatedLine);
                }
                curveLoop = translatedCurveLoop;

                //Solid cube = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                Solid cube = GeometryCreationUtilities.CreateExtrusionGeometry(new CurveLoop[] { curveLoop }, XYZ.BasisZ, dimension);

                using (Transaction t = new Transaction(doc, "Create cube direct shape"))
                {
                    t.Start();
                    // Tạo một đối tượng DirectShape trong tài liệu Revit.
                    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                    ds.ApplicationId = "Application id";
                    ds.ApplicationDataId = "Geometry object id";
                    ds.SetShape(new GeometryObject[] { cube }); // Sử dụng DirectShape.SetShape để gán hình lập phương vào DirectShape.
                    TaskDialog.Show("Tạo khối lập phương", "Đã tạo thành công hình lập phương" + "\n" +
                                    "Category Name:  " + ds.Category.Name);
                    t.Commit();
                }
            }
        }

        private void CreateSphere(Document doc, double dimension)
        {
            List<Curve> profile = new List<Curve>();
            dimension = Dimension;

            // first create sphere with dimension = radius
            XYZ center = XYZ.Zero;
            double radius = dimension;
            XYZ profile00 = center;
            XYZ profilePlus = center + new XYZ(0, radius, 0);
            XYZ profileMinus = center - new XYZ(0, radius, 0);

            profile.Add(Line.CreateBound(profilePlus, profileMinus));
            profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

            //Tạo CurveLoop từ danh sách các đường cong để đại diện cho đường viền của hình cầu.
            CurveLoop curveLoop = CurveLoop.Create(profile);

            // Di chuyển CurveLoop để có tâm là điểm giao nhau
            CurveLoop translatedCurveLoop = new CurveLoop();

            foreach (Curve curve in curveLoop)
            {
                Curve translatedCurve = curve.CreateTransformed(Transform.CreateTranslation(GetCreatePoint(doc)));
                translatedCurveLoop.Append(translatedCurve);
            }

            //Tạo SolidOptions để chỉ định các tùy chọn cho việc tạo hình cầu.
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            //Tạo Frame để xác định vị trí và hướng của hình cầu.
            Frame frame = new Frame(GetCreatePoint(doc), XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);

            //Kiểm tra xem Frame có thể định nghĩa hình học trong Revit hay không
            if (Frame.CanDefineRevitGeometry(frame) == true)
            {
                try
                {
                    Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { translatedCurveLoop }, 0, 2 * Math.PI, options);
                    using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
                    {
                        t.Start();

                        //tạo một đối tượng DirectShape trong tài liệu Revit
                        DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                        ds.ApplicationId = "Application id";
                        ds.ApplicationDataId = "Geometry object id";
                        ds.SetShape(new GeometryObject[] { sphere }); //Sử dụng DirectShape.SetShape để gán hình cầu vào DirectShape.
                        TaskDialog.Show("Tạo hình cầu", "Đã tạo thành công hình cầu" + "\n" +
                                        "Category Name:  " + ds.Category.Name);
                        t.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private List<Grid> getAllGrid(Document doc)
        {
            return new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                    .OfCategory(BuiltInCategory.OST_Grids)
                                                    .OfClass(typeof(Grid))
                                                    .Cast<Grid>()
                                                    .ToList();
        }

        private XYZ GetIntersection(Grid g1, Grid g2)
        {
            if (g1 != null && g2 != null)
            {
                var ret = g1.Curve.Intersect(g2.Curve, out IntersectionResultArray result);
                if (ret == SetComparisonResult.Overlap && result.Size == 1)
                {
                    return result.get_Item(0).XYZPoint;
                }
            }
            return null;
        }

        private XYZ GetCreatePoint(Document doc)
        {
            //lay toan bo grid trong project
            List<Grid> grids = getAllGrid(doc);

            //tim giao diem cua 2 grid
            List<XYZ> points = new List<XYZ>();
            for (int i = 0; i < grids.Count; i++)
            {
                for (int j = i + 1; j < grids.Count; j++)
                {
                    XYZ p = GetIntersection(grids[i], grids[j]);
                    if (p != null)
                        points.Add(p);
                }
            }
            if (points.Count == 1)
                return points[0];
            else if (points.Count > 1)
            {
                return SelectMultiIntersect(doc, points);
            }
            else
            {
                TaskDialog.Show("Lỗi", "Không tìm thấy giao điểm");
                return null;
            }
        }

        private XYZ SelectMultiIntersect(Document doc, List<XYZ> points)
        {
            UIDocument uidoc = new UIDocument(doc);
            Selection choices = uidoc.Selection;

            XYZ selectedPoint = null;

            // danh sách các tùy chọn
            IList<string> options = new List<string>();

            foreach (XYZ point in points)
            {
                string option = $"Giao điểm: ({point.X}, {point.Y}, {point.Z})";
                options.Add(option);
            }

            // Hiển thị dialog
            TaskDialog dialog = new TaskDialog("Chọn giao điểm");
            dialog.MainInstruction = "Chọn giao điểm để đặt family";
            dialog.MainContent = "Các giao điểm có sẵn:";
            dialog.CommonButtons = TaskDialogCommonButtons.Cancel;

            // Thêm tùy chọn vào dialog
            foreach (string option in options)
            {
                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1 + options.IndexOf(option), option);
            }

            // Hiển thị dialog và xử lý lựa chọn
            TaskDialogResult result = dialog.Show();

            if (result != TaskDialogResult.Cancel)
            {
                int selectedIndex = (int)result - (int)TaskDialogCommandLinkId.CommandLink1;
                if (selectedIndex >= 0 && selectedIndex < points.Count)
                {
                    selectedPoint = points[selectedIndex];
                }
            }
            if (result == TaskDialogResult.Cancel)
            {
                return null;
            }

            return selectedPoint;
        }
    }
}