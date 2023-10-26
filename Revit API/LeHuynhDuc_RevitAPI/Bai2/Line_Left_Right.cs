using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    internal class Line_Left_Right : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Lấy tham chiếu đến ứng dụng Revit hiện tại
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            try
            {
                // Cho người dùng pick 2 điểm
                XYZ startPoint = uiDoc.Selection.PickPoint("Pick the first point");

                XYZ endPoint = uiDoc.Selection.PickPoint("Pick the second point");

                // Tạo đường thằng từ 2 đường đã pick
                Line line = Line.CreateBound(startPoint, endPoint);

                //Tạo đường thẳng trên revit
                using (Transaction tx = new Transaction(doc, "Create Line"))
                {
                    tx.Start();

                    // Use the Create.NewModelCurve method to create the line
                    ModelCurve modelCurve = doc.Create.NewModelCurve(line, SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(uiDoc.ActiveView.ViewDirection, startPoint)));

                    tx.Commit();
                }
                // int d = 0;
                try
                {
                    while (startPoint != null && endPoint != null)
                    {
                        XYZ Point = uiDoc.Selection.PickPoint("Pick the point");
                        double td = (startPoint.Y + Point.Y) / 2;
                        bool Right = CheckLR(Point, startPoint, endPoint), t = CheckT(startPoint, endPoint, Point);
                        if (t)
                        {
                            TaskDialog.Show("Thong bao", "Nam tren duong thang");
                        }
                        else if (Dao(startPoint, endPoint))
                        {
                            if (Right)
                            {
                                TaskDialog.Show("Thong bao", "Right");
                            }
                            else
                            {
                                TaskDialog.Show("Thong bao", "Left");
                            }
                        }
                        else
                        {
                            if (!Right)
                            {
                                TaskDialog.Show("Thong bao", "Right");
                            }
                            else
                            {
                                TaskDialog.Show("Thong bao", "Left");
                            }
                        }

                        // d++;
                    }
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return Result.Failed;
                }



            }
            catch (Exception ex1)
            {
                message = ex1.Message;
                return Result.Failed;
            }
           

            
        }
        //Kiem tra thang hang 
        private bool CheckT(XYZ a, XYZ b, XYZ c)
        {
            double ab, ac, bc;
            ab = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
            ac = Math.Sqrt((a.X - c.X) * (a.X - c.X) + (a.Y - c.Y) * (a.Y - c.Y));
            bc = Math.Sqrt((c.X - b.X) * (c.X - b.X) + (c.Y - b.Y) * (c.Y - b.Y));

            if (ab + bc <= ac || ab + ac <= bc || ac + bc <= ab)
                return true;
            else return false;
        }
        //Dung tich vo huong cua 2 vector
        private bool CheckLR(XYZ a, XYZ b, XYZ c)
        {
            double position = (c.Y - b.Y) * (a.X - b.X) - (c.X - b.X) * (a.Y - b.Y);
            return position > 0;
        }
        //Kiem tra hướng của vector a -> b (startpoint -> endpoint)
        private bool Dao(XYZ a, XYZ b)
        {
            if (a.X < b.X && a.Y < b.Y) return true;
            if (a.X >= b.X && a.Y < b.Y) return true;
            if (a.X > b.X && a.Y > b.Y) return false;
            if (a.X <= b.X && a.Y > b.Y) return false;
            return false;
        }
    }

}
