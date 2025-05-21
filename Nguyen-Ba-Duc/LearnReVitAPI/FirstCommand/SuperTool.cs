using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FirstCommand.View;
using OpenQA.Selenium;
using static Autodesk.Revit.DB.SpecTypeId;

namespace FirstCommand
{
    [Transaction(TransactionMode.Manual)]
    public class SuperTool : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Tạo handler và external event
            MyRevitHandler handler = new MyRevitHandler();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            // Mở UI modeless
            SuperToolWindow viewWindow = new SuperToolWindow(exEvent, handler);
            // Lấy handle của cửa sổ Revit
            IntPtr revitHandle = Autodesk.Windows.ComponentManager.ApplicationWindow;

            // Gắn Revit làm owner
            WindowInteropHelper helper = new WindowInteropHelper(viewWindow);
            helper.Owner = revitHandle;

            // (Tuỳ chọn) Nếu muốn luôn nổi trên cửa sổ Revit
            //viewWindow.Topmost = true;
            viewWindow.Show();

            return Result.Succeeded;
        }
    }

    public class MyRevitHandler : IExternalEventHandler
    {
        public string StartPoint { get; set; }
        public string MidPoint { get; set; }
        public string EndPoint { get; set; }
        public bool IsLine { get; set; }
        public bool IsPoint { get; set; }
        public bool IsArc { get; set; }

        public bool IsShow { get; set; }
        public string IdInput { get; set; } = "";

        public void Execute(UIApplication app)
        {
            UIDocument uiDoc = app.ActiveUIDocument;
            Document doc = uiDoc.Document;

            if (!string.IsNullOrWhiteSpace(IdInput))
            {
                List<string> listIds = IdInput.Split(';')
                         .Select(s => s.Trim())
                         .ToList();
                IdInput = "";

                SelectAndShowElementsByIdList(uiDoc, listIds, IsShow);
                IsShow = false;
            }

            if (IsLine)
            {
                var listTuplePoints = new List<(XYZ, XYZ)>();

                listTuplePoints.Add((ParseXYZFromString(StartPoint), ParseXYZFromString(EndPoint)));

                ModelCurve modelCurve = CreateModelLineAutoPlane(uiDoc, doc, listTuplePoints, false);
                uiDoc.ShowElements(modelCurve.Id);
                IsLine = false;
            }
            else if (IsArc)
            {
                CreateModelArcFrom3Points(uiDoc, doc, ParseXYZFromString(StartPoint), ParseXYZFromString(MidPoint), ParseXYZFromString(EndPoint));

                IsArc = false;
            }
            else if (IsPoint)
            {
                List<string> listStringPoints = new List<string>();
                List<ElementId> listModelCurveIds = new List<ElementId>();

                if (StartPoint != "")
                {
                    listStringPoints.AddRange(GetPointsFromString(StartPoint));
                }
                if (MidPoint != "")
                {
                    listStringPoints.AddRange(GetPointsFromString(MidPoint));
                }
                if (EndPoint != "")
                {
                    listStringPoints.AddRange(GetPointsFromString(EndPoint));
                }

                foreach (var stringPoint in listStringPoints)
                {
                    var tuplePoints = CreatePointForModelLineByPoint(ParseXYZFromString(stringPoint));
                    ModelCurve modelCurve = CreateModelLineAutoPlane(uiDoc, doc, tuplePoints, true);
                    listModelCurveIds.Add(modelCurve.Id);
                }
                uiDoc.ShowElements(listModelCurveIds);
                IsPoint = false;
            }
        }

        private List<string> GetPointsFromString(string stringPoints)
        {
            return stringPoints.Split(';')
                         .Select(s => s.Trim())
                         .ToList();
        }

        public void SelectAndShowElementsByIdList(UIDocument uidoc, List<string> idStrings, bool IsShow)
        {
            Document doc = uidoc.Document;
            List<ElementId> validIds = new List<ElementId>();

            foreach (string idStr in idStrings)
            {
                if (int.TryParse(idStr.Trim(), out int idInt))
                {
                    ElementId eid = new ElementId(idInt);
                    Element element = doc.GetElement(eid);
                    if (element != null)
                        validIds.Add(eid);
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

        public XYZ ParseXYZFromString(string input)
        {
            // Dùng regex để bắt x, y, z từ chuỗi như "(1.2, 3.4, 5.6)" hoặc "{1.2, 3.4, 5.6}"
            //var regex = new Regex(@"[\(\{]\s*([-+]?[0-9]*\.?[0-9]+)\s*,\s*([-+]?[0-9]*\.?[0-9]+)\s*,\s*([-+]?[0-9]*\.?[0-9]+)\s*[\)\}]");
            var regex = new Regex(@"[\{\(]*\s*([-+]?[0-9]*\.?[0-9]+)\s*,\s*([-+]?[0-9]*\.?[0-9]+)\s*,\s*([-+]?[0-9]*\.?[0-9]+)\s*[\}\)]*", RegexOptions.Compiled);

            var match = regex.Match(input);

            if (!match.Success || match.Groups.Count != 4)
                throw new FormatException("Chuỗi không đúng định dạng (x, y, z) hoặc {x, y, z}");

            // Phân tích số với InvariantCulture để dùng dấu chấm làm phân cách thập phân
            double x = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            double y = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            double z = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            return new XYZ(x, y, z);
        }

        private List<(XYZ p1, XYZ p2)> CreatePointForModelLineByPoint(XYZ point)
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

        public ModelCurve CreateModelLineAutoPlane(UIDocument uiDoc, Document doc, List<(XYZ p1, XYZ p2)> listTuples, bool isPoint)
        {
            ModelCurve modelCurve = null;
            using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            {
                trans.Start();

                foreach (var tuple in listTuples)
                {
                    XYZ pt1 = tuple.p1 as XYZ;
                    XYZ pt2 = tuple.p2 as XYZ;

                    Line line = Line.CreateBound(pt1, pt2);
                    XYZ direction = (pt2 - pt1).Normalize();

                    // Xác định xem vector direction có gần song song với các trục X, Y, Z hay không
                    bool isParallelToX = Math.Abs(direction.DotProduct(XYZ.BasisX)) > 0.99;
                    bool isParallelToY = Math.Abs(direction.DotProduct(XYZ.BasisY)) > 0.99;
                    bool isParallelToZ = Math.Abs(direction.DotProduct(XYZ.BasisZ)) > 0.99;

                    Plane plane;

                    if (isPoint)
                    {
                        plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX.CrossProduct(XYZ.BasisY), pt1);
                    }
                    else if (isParallelToX)
                    {
                        // Song song trục X → dùng mặt phẳng YZ
                        plane = Plane.CreateByNormalAndOrigin(XYZ.BasisY, pt1);
                    }
                    else if (isParallelToY)
                    {
                        // Song song trục Y → dùng mặt phẳng XZ
                        plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, pt1);
                    }
                    else if (isParallelToZ)
                    {
                        // Song song trục Z → dùng mặt phẳng XY
                        plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, pt1);
                    }
                    else
                    {
                        // Không song song với X/Y/Z → dùng trục Z để tạo normal qua cross product
                        XYZ normal = direction.CrossProduct(XYZ.BasisZ).Normalize();

                        plane = Plane.CreateByNormalAndOrigin(normal, pt1);
                    }

                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    modelCurve = doc.Create.NewModelCurve(line, sketchPlane);

                    //OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    //ogs.SetProjectionLineColor(new Color(255, 0, 0));

                    //doc.ActiveView.SetElementOverrides(modelCurve.Id, ogs);
                }

                trans.Commit();
            }
            //uiDoc.ShowElements(modelCurve.Id);
            return modelCurve;
        }

        public void CreateModelArcFrom3Points(UIDocument uiDoc, Document doc, XYZ p1, XYZ p2, XYZ p3)
        {
            ModelCurve modelCurve = null;
            using (Transaction trans = new Transaction(doc, "Create Model Arc From 3 Points"))
            {
                trans.Start();

                // Tạo cung từ 3 điểm
                Arc arc = Arc.Create(p1, p2, p3);

                // Tính mặt phẳng chứa cung: pháp tuyến = tích có hướng giữa 2 vector bất kỳ trên mặt cong
                XYZ v1 = (p2 - p1).Normalize();
                XYZ v2 = (p3 - p1).Normalize();
                XYZ normal = v1.CrossProduct(v2).Normalize();

                // Kiểm tra normal hợp lệ
                if (normal.IsZeroLength())
                    throw new InvalidOperationException("3 điểm thẳng hàng – không thể tạo cung.");

                Plane plane = Plane.CreateByNormalAndOrigin(normal, p1);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                modelCurve = doc.Create.NewModelCurve(arc, sketchPlane);

                trans.Commit();
            }
            uiDoc.ShowElements(modelCurve.Id);
        }

        public string GetName() => "Draw Line or Arc";
    }
}