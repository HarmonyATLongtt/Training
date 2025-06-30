using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using FirstCommand.Support.TransactionHandle;
using FirstCommand.Support.LineHandle;

namespace FirstCommand.Support.DrawOnRevit
{
    public static class DrawPointLineArc
    {
        /// <summary>
        /// Vẽ modelline mới từ 2 điểm bất kỳ
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        public static void CreateModelLine(Document doc, XYZ p1, XYZ p2, bool isRevitLink, Transform transform, double multiply)
        {
            //using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            //{
            //trans.Start();
            HandleForTransaction.RunTransaction(doc, "Create Model Line", (Transaction t) =>
            {
                ModelCurve modelCurve = null;

                if (isRevitLink)
                {
                    p1 = transform.OfPoint(p1);
                    p2 = transform.OfPoint(p2);
                }

                //XYZ pt1 = transform.OfPoint(point1);
                //XYZ pt2 = transform.OfPoint(point2);

                //XYZ newVector = XYZ.BasisZ.Multiply(1);
                //XYZ p1 = pt1.Add(newVector);
                //XYZ p2 = pt2.Add(newVector);

                if (multiply > 0)
                {
                    XYZ newVector = XYZ.BasisZ.Multiply(1);
                    p1 = p1.Add(newVector);
                    p2 = p2.Add(newVector);
                }

                Line line = Line.CreateBound(p1, p2);
                XYZ direction = (p2 - p1).Normalize();

                bool isParallelToX = Math.Abs(direction.DotProduct(XYZ.BasisX)) > 0.99;
                bool isParallelToY = Math.Abs(direction.DotProduct(XYZ.BasisY)) > 0.99;
                bool isParallelToZ = Math.Abs(direction.DotProduct(XYZ.BasisZ)) > 0.99;

                Plane plane;

                if (isParallelToX)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisY, p1);
                }
                else if (isParallelToY)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else if (isParallelToZ)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else
                {
                    XYZ normal = direction.CrossProduct(XYZ.BasisZ).Normalize();
                    plane = Plane.CreateByNormalAndOrigin(normal, p1);
                }
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                if (isParallelToX || isParallelToY || isParallelToZ)
                {
                    Line snappedLine = LineUtility.ProjectLineOntoSketchPlane(line, plane);
                    if (snappedLine != null)
                    {
                        modelCurve = doc.Create.NewModelCurve(snappedLine, sketchPlane);
                    }
                    else
                    {
                        //TaskDialog.Show("Lỗi", "Line không nằm gần SketchPlane. Không thể vẽ.");
                    }
                }
                else
                {
                    modelCurve = doc.Create.NewModelCurve(line, sketchPlane);
                }
                //Random random = new Random();

                //// Tạo giá trị RGB ngẫu nhiên từ 0 đến 255
                //byte red = (byte)random.Next(0, 256);
                //byte green = (byte)random.Next(0, 256);
                //byte blue = (byte)random.Next(0, 256);

                //OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                //ogs.SetProjectionLineColor(new Color(red, green, blue));

                //doc.ActiveView.SetElementOverrides(modelCurve.Id, ogs);
            });
            //trans.Commit();
            //}
        }

        /// <summary>
        /// Hàm vẽ Arc từ 3 điểm bất kỳ
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="doc"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void CreateModelArcFrom3Points(UIDocument uiDoc, Document doc, XYZ p1, XYZ p2, XYZ p3, bool isRevitLink, Transform transform, double multiply)
        {
            ModelCurve modelCurve = null;
            using (Transaction trans = new Transaction(doc, "Create Model Arc From 3 Points"))
            {
                trans.Start();

                if (isRevitLink)
                {
                    p1 = transform.OfPoint(p1);
                    p2 = transform.OfPoint(p2);
                    p3 = transform.OfPoint(p3);
                }

                //XYZ pt1 = transform.OfPoint(point1);
                //XYZ pt2 = transform.OfPoint(point2);

                //XYZ newVector = XYZ.BasisZ.Multiply(1);
                //XYZ p1 = pt1.Add(newVector);
                //XYZ p2 = pt2.Add(newVector);

                if (multiply > 0)
                {
                    XYZ newVector = XYZ.BasisZ.Multiply(1);
                    p1 = p1.Add(newVector);
                    p2 = p2.Add(newVector);
                    p3 = p3.Add(newVector);
                }

                //XYZ p1 = transform.OfPoint(point1);
                //XYZ p2 = transform.OfPoint(point2);
                //XYZ p3 = transform.OfPoint(point3);

                //XYZ newVector = XYZ.BasisZ.Multiply(1);
                //XYZ pt1 = p1.Add(newVector);
                //XYZ pt2 = p2.Add(newVector);
                //XYZ pt3 = p3.Add(newVector);

                // Tạo cung từ 3 điểm
                Arc arc = Arc.Create(p1, p2, p3);

                // Tính mặt phẳng chứa cung: pháp tuyến = tích có hướng giữa 2 vector bất kỳ trên mặt cong
                XYZ v1 = (p2 - p1).Normalize();
                XYZ v2 = (p3 - p1).Normalize();
                XYZ normal = v1.CrossProduct(v2).Normalize();

                // Kiểm tra normal hợp lệ
                if (normal.IsZeroLength())
                    throw new System.InvalidOperationException("3 điểm thẳng hàng – không thể tạo cung.");

                Plane plane = Plane.CreateByNormalAndOrigin(normal, p1);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                modelCurve = doc.Create.NewModelCurve(arc, sketchPlane);

                trans.Commit();
            }
            uiDoc.ShowElements(modelCurve.Id);
        }
    }
}