using Autodesk.Revit.DB;
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

        private void CreateCube(Document doc, double dimension)
        {
            List<Curve> profile = new List<Curve>();
            dimension = Dimension;

            //Grid grid1;
            //Grid grid2;
            //grid1.Curve is Line line1;  
            //grid2.Curve

            //IntersectionResultArray intersectionResults = new IntersectionResultArray();

            //List<XYZ> points = new List<XYZ>();

            //SetComparisonResult result = line1.Intersect(line2, out intersectionResults);

            //if (result == SetComparisonResult.Overlap)
            //{
            //    foreach (IntersectionResult intersectionResult in intersectionResults)
            //    {
            //        XYZ point = intersectionResult.XYZPoint;
            //        points.Add(point);
            //    }
            //}

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
            // Đặt trung tâm tại điểm XYZ.Zero và xác định hướng trục X, trục Z và trục Y.
            Frame frame = new Frame(pointA, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame) == true) // Kiểm tra xem Frame có thể định nghĩa hình học trong Revit hay không.
            {
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

            //Tạo SolidOptions để chỉ định các tùy chọn cho việc tạo hình cầu.
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            //Tạo Frame để xác định vị trí và hướng của hình cầu.
            //đặt trung tâm tại điểm XYZ.Zero và xác định hướng trục X, trục Z và trục Y.
            Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
            if (Frame.CanDefineRevitGeometry(frame) == true) //Kiểm tra xem Frame có thể định nghĩa hình học trong Revit hay không
            {
                Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(frame, new CurveLoop[] { curveLoop }, 0, 2 * Math.PI, options);
                using (Transaction t = new Transaction(doc, "Create sphere direct shape"))
                {
                    t.Start();
                    //tạo một đối tượng DirectShape trong tài liệu Revit
                    DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));

                    ds.ApplicationId = "Application id";
                    ds.ApplicationDataId = "Geometry object id";
                    ds.SetShape(new GeometryObject[] { sphere }); //Sử dụng DirectShape.SetShape để gán hình cầu vào DirectShape.
                    t.Commit();
                }
            }
        }
    }
}