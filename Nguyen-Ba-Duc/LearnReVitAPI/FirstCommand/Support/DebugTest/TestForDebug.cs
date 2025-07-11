using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstCommand.Support.TransactionHandle;
using FirstCommand.Support.LineHandle;
using System.Linq;
using FirstCommand.Support.PointHandle;

namespace FirstCommand.Support.DebugTest
{
    public static class TestForDebug
    {
        /// <summary>
        /// Hiển thị danh sách các giá trị theo từng dòng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void ShowValues<T>(List<T> list)
        {
            string str = "";
            foreach (var l in list)
            {
                str += l.ToString();
                str += "\n";
            }
            TaskDialog.Show("Nofi", str);
        }

        /// <summary>
        /// Hiển thị danh sách các điểm thành 1 string có ngăn cách bởi dấu chấm phẩy
        /// </summary>
        /// <param name="points"></param>
        public static void ShowPointsToDraw(List<XYZ> points)
        {
            string str = "";
            foreach (var p in points)
            {
                str += p.ToString();
                str += ";";
            }
            TaskDialog.Show("Nofi", str);
        }

        /// <summary>
        /// Tạo Directshape từ solid
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid"></param>
        public static void CreateDirectShapeFromSolid(Document doc, Solid solid)
        {
            HandleForTransaction.RunTransaction(doc, "Create DirectShape", (Transaction t) =>
            {
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new List<GeometryObject> { solid });
            });
        }

        public static List<string> GetListStringFromListObject<T>(List<T> list)
        {
            List<string> strings = new List<string>();
            foreach (var item in list)
            {
                strings.Add(item.ToString());
            }
            return strings;
        }

        /// <summary>
        /// Hàm dùng để show ra danh sách các id
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="idStrings"></param>
        /// <param name="IsShow"></param>
        public static void SelectAndShowElementsByIdList(UIDocument uidoc, List<ElementId> elementIds, ElementId elementId, bool IsShow)
        {
            Document doc = uidoc.Document;
            List<ElementId> validIds = new List<ElementId>();

            if (elementIds != null && elementIds.Count > 0)
            {
                foreach (ElementId id in elementIds)
                {
                    if (IsValidId(doc, id))
                    {
                        validIds.Add(id);
                    }
                }
            }
            else if (elementId != null)
            {
                if (IsValidId(doc, elementId))
                {
                    validIds.Add(elementId);
                }
            }
            if (validIds.Count > 0)
            {
                if (IsShow)
                {
                    uidoc.Selection.SetElementIds(validIds);
                    uidoc.ShowElements(validIds);
                }
                else
                {
                    uidoc.Selection.SetElementIds(validIds);
                }
            }
            else
            {
                TaskDialog.Show("Warning", "Không tìm thấy phần tử nào hợp lệ.");
            }
        }

        /// <summary>
        /// Hàm kiểm tra xem có Element nào tồn tại không với Id được truyền vào
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private static bool IsValidId(Document doc, ElementId id)
        {
            Element element = doc.GetElement(id);
            if (element != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Hàm dùng để vẽ points trên Revit
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="points"></param>
        public static void DrawPoints(Document doc, List<XYZ> points = null, XYZ point = null)
        {
            if (points != null && points.Count > 0)
            {
                foreach (XYZ p in points)
                {
                    var tuplePoints = CreatePointsForModelLineByPoint(p);
                    CreateModelLineAutoPlane(doc, tuplePoints);
                }
            }
            else if (point != null)
            {
                var tuplePoints = CreatePointsForModelLineByPoint(point);
                CreateModelLineAutoPlane(doc, tuplePoints);
            }
        }

        private static List<(XYZ p1, XYZ p2)> CreatePointsForModelLineByPoint(XYZ point)
        {
            var tuplePoints = new List<(XYZ p1, XYZ p2)>();
            XYZ pointOnAsisX1 = new XYZ(point.X - 0.5, point.Y, point.Z);
            XYZ pointOnAsisX2 = new XYZ(point.X + 0.5, point.Y, point.Z);

            tuplePoints.Add((pointOnAsisX1, pointOnAsisX2));

            XYZ pointOnAsisY1 = new XYZ(point.X, point.Y - 0.5, point.Z);
            XYZ pointOnAsisY2 = new XYZ(point.X, point.Y + 0.5, point.Z);

            tuplePoints.Add((pointOnAsisY1, pointOnAsisY2));
            return tuplePoints;
        }

        private static void CreateModelLineAutoPlane(Document doc, List<(XYZ p1, XYZ p2)> listTuples)
        {
            using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            {
                trans.Start();

                foreach (var tuple in listTuples)
                {
                    XYZ pt1 = tuple.p1 as XYZ;
                    XYZ pt2 = tuple.p2 as XYZ;

                    Line line = Line.CreateBound(pt1, pt2);
                    XYZ direction = (pt2 - pt1).Normalize();

                    Plane plane;

                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX.CrossProduct(XYZ.BasisY), pt1);

                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                    ModelCurve modelCurve = doc.Create.NewModelCurve(line, sketchPlane);
                }

                trans.Commit();
            }
        }
    }
}