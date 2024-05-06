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

            XYZ newPoint = new XYZ(0, 0, 0);

            // Tạo SolidOptions để chỉ định các tùy chọn cho việc tạo hình lập phương.
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            // Tạo Frame để xác định vị trí và hướng của hình lập phương.
            if (GetCreatePoint(doc) != null)
            {
                foreach (var item in GetCreatePoint(doc))
                {
                    newPoint = item;
                }
            }
            else
            {
                TaskDialog.Show("Không có giao điểm", "Không có giao điểm, đặt khối vào vị trí (0,0,0)");
            }
            Frame frame = new Frame(newPoint, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);

            if (Frame.CanDefineRevitGeometry(frame) == true) // Kiểm tra xem Frame có thể định nghĩa hình học trong Revit hay không.
            {
                // Tính toán vector dịch chuyển từ điểm ban đầu của CurveLoop đến điểm mới
                XYZ translationVector = newPoint;

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

            XYZ newpoint = new XYZ();

            foreach (Curve curve in curveLoop)
            {
                foreach (var item in GetCreatePoint(doc))
                {
                    newpoint = item;
                    Curve translatedCurve = curve.CreateTransformed(Transform.CreateTranslation(newpoint));
                    translatedCurveLoop.Append(translatedCurve);
                }
            }

            //Tạo SolidOptions để chỉ định các tùy chọn cho việc tạo hình cầu.
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            //Tạo Frame để xác định vị trí và hướng của hình cầu.
            Frame frame = new Frame(newpoint, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);

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

        private List<XYZ> GetCreatePoint(Document doc)
        {
            //lay toan bo grid trong project
            List<Grid> grids = getAllGrid(doc);

            Dictionary<string, XYZ> dic = new Dictionary<string, XYZ>();

            //tim giao diem cua 2 grid
            List<XYZ> points = new List<XYZ>();
            for (int i = 0; i < grids.Count; i++)
            {
                for (int j = i + 1; j < grids.Count; j++)
                {
                    XYZ p = GetIntersection(grids[i], grids[j]);
                    if (p != null)
                        dic.Add(grids[i].Name + "-" + grids[j].Name, p);
                }
            }
            if (dic.Count == 1 && dic.Count <= 1)
                return dic.Values.ToList();
            else if (dic.Count > 1)
            {
                TaskDialogResult result = (TaskDialogResult)ShowDialogPickPoint();

                if (result == TaskDialogResult.CommandLink1) //chọn "Đặt 1 giao điểm"
                    return new List<XYZ> { SelectMultiIntersect(doc, dic) };
                else if (result == TaskDialogResult.CommandLink2) //chọn "Đặt tất cả giao điểm"
                    return dic.Values.ToList();
                else
                    return null;
            }
            else
            {
                TaskDialog.Show("Lỗi", "Không tìm thấy giao điểm");
                return null;
            }
        }

        private XYZ SelectMultiIntersect(Document doc, Dictionary<string, XYZ> points)
        {
            UIDocument uidoc = new UIDocument(doc);
            Selection choices = uidoc.Selection;

            XYZ selectedPoint = null;

            // danh sách các tùy chọn
            List<string> options = new List<string>();

            foreach (var dic in points)
            {
                string option = $"Giao điểm: " + dic.Key;
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
                    selectedPoint = points.Values.ToList()[selectedIndex];
                }
            }
            if (result == TaskDialogResult.Cancel)
            {
                return null;
            }

            return selectedPoint;
        }

        public object ShowDialogPickPoint()
        {
            TaskDialog dialog = new TaskDialog("Lựa chọn đặt family");
            dialog.MainInstruction = "Chọn lựa chọn đặt family";
            dialog.MainContent = "lựa chọn đặt family lên 1 giao điểm hoặc tất cả giao điểm đã tìm thấy.";

            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Đặt 1 giao điểm");

            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Đặt tất cả giao điểm");

            TaskDialogResult result = dialog.Show();

            return result;
        }
    }
}