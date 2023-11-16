using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using toDuc26102023.Models;

namespace toDuc26102023.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_ToDuc_26102023 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;
                Document doc = uiDoc.Document;

                IList<Reference> listRe = uiDoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element);
                //list các line đã chọn
                List<ModelLine> lines = new List<ModelLine>();
                //List các điểm của line khép kín (lấy 1 đầu mút)
                List<XYZ> points = new List<XYZ>();
                //List các điểm của line khép kín (lấy 2 đầu mút)
                List<XYZ> points1 = new List<XYZ>();
                //List tam giác đã tìm được 
                List<Tamgiac> tamgiacs = new List<Tamgiac>();
                //List các cạnh của tam giác đã tìm được
                List<Line> ListLine = new List<Line>();
                List<Line> ListLine1 = new List<Line>();
                //Tổng diện tích của vùng khép kín bởi line đã chọn
                double TongDienTich = 0;
                //solid được tạo từ các line đã chọn
                Solid fullSolid;
                using (TransactionGroup transg = new TransactionGroup(doc, "DatTamGiac"))
                {
                    transg.Start();
                    //Lấy ra các điểm của Line khép kín
                    foreach (Reference reference in listRe)
                    {
                        ElementId elementid1 = reference.ElementId;
                        Element element = doc.GetElement(elementid1);
                        if (element is ModelLine)
                        {
                            
                            //lấy ra các điểm và modeline cho vào list từ các line đã chọn
                            ModelLine line = element as ModelLine;
                            lines.Add(line);
                            Curve curve = line.GeometryCurve;
                            ListLine1.Add(curve as Line);
                            points.Add(curve.GetEndPoint(0));
                            points.Add(curve.GetEndPoint(1));
                            points1.Add(curve.GetEndPoint(0));
                            points1.Add(curve.GetEndPoint(1));
                        }

                    }
                    //tạo solid từ các line khép kín đã chọn
                    CurveLoop curveLoop = new CurveLoop();
                    List<Line> defaultLine = new List<Line>();
                    Line linetg = null;
                    int d = 0;
                    while(d != ListLine1.Count)
                    {
                        if (defaultLine.Count == 0)
                        {
                            defaultLine.Add(ListLine1[0]);
                            linetg = ListLine1[0];
                        }
                        else
                        {
                            foreach(Line line in ListLine1)
                            {
                                if (equalPoints(linetg.GetEndPoint(1), line.GetEndPoint(0)))
                                { 
                                    linetg = line;
                                    defaultLine.Add(linetg);
                                    break;
                                } 
                                else if (equalPoints(linetg.GetEndPoint(1), line.GetEndPoint(1)) 
                                                      && equalPoints(linetg.GetEndPoint(0), line.GetEndPoint(0))==false)
                                {
                                    linetg = Line.CreateBound(line.GetEndPoint(1), line.GetEndPoint(0));      
                                    defaultLine.Add(linetg);
                                    break;
                                }
                            }
                        }
                        d++;
                    }
                    foreach(Line line in defaultLine)
                    {
                        try
                        {
                            curveLoop.Append(line);
                        }
                        catch (Exception ex)
                        {
                            curveLoop.Append(Line.CreateBound(line.GetEndPoint(1), line.GetEndPoint(0)));
                        }
                    }


                    fullSolid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { curveLoop }, XYZ.BasisZ, 10);
                    foreach (Face f in fullSolid.Faces)
                    {
                        XYZ normal = f.ComputeNormal(new UV(0, 0));
                        if (normal.IsAlmostEqualTo(new XYZ(0, 0, 1)))
                        {
                            TongDienTich = f.Area;
                            break;
                        }
                    }
                    //dien tích mặt solid 
                    TongDienTich = Math.Round(TongDienTich, 7);

                    //Sắp xếp modeline theo chiều giảm dần của length
                    LineLengthComparer comparer = new LineLengthComparer();
                    lines.Sort(comparer);
                    //Duyệt qua từng modeline
                    foreach (ModelLine line in lines)
                    {
                        
                        //Lấy ra 2 điểm mút của modeline
                        XYZ p1 = line.GeometryCurve.GetEndPoint(0);
                        XYZ p2 = line.GeometryCurve.GetEndPoint(1);
                        XYZ point1 = new XYZ(-999999999, 999999999, 9999999999);
                        double maxkc = 0;
                        foreach (XYZ point in points)
                        {
                            //Tìm điểm còn lại của tam giác phù hợp điều kiện (khoảng cách xa nhất, 
                            Tamgiac newtg = new Tamgiac(point, p1, p2);
                            if (CheckTamGiac(newtg.Dinh1, newtg.Dinh2, newtg.Dinh3))
                            if (maxkc < KC(line, point)
                                && checkGiaoNhau(lines, p1, point)
                                && checkGiaoNhau(lines, p2, point)
                                && equalPoints(point, p1) == false
                                && equalPoints(point, p2) == false
                                && checkGiaoTamGiac(ListLine, p1, point)
                                && checkGiaoTamGiac(ListLine, p2, point)
                                && Math.Round(newtg.DienTich(), 7) == Math.Round(DienTichGiaoNhau(fullSolid, SolidTamGiac(newtg)), 7)
                                )
                            {
                                maxkc = KC(line, point);
                                point1 = point;
                            }

                        }

                        Tamgiac tg = new Tamgiac(p1, p2, point1);
                        if (CheckTrungTG(ListLine, tg) && equalPoints(point1, new XYZ(-999999999, 999999999, 9999999999))==false)
                        {
                            TongDienTich -= Math.Round(tg.DienTich(), 7);
                            tamgiacs.Add(tg);
                            Line l1 = Line.CreateBound(tg.Dinh1, tg.Dinh2);
                            Line l2 = Line.CreateBound(tg.Dinh2, tg.Dinh3);
                            Line l3 = Line.CreateBound(tg.Dinh3, tg.Dinh1);
                            ListLine.Add(l1);
                            ListLine.Add(l2);
                            ListLine.Add(l3);
                        }
                    }
                    if (TongDienTich > 0)
                    {
                        List<Tamgiac> ListTamGiac = new List<Tamgiac>();
                        for (int i = 0; i < points.Count; i++)
                            for (int j = 0; j < points.Count; j++)
                                for (int k = 0; k < points.Count; k++)
                                {
                                    if (equalPoints(points[i], points[j]) == false
                                        && equalPoints(points[i], points[k]) == false
                                        && equalPoints(points[j], points[k]) == false
                                        )
                                        ListTamGiac.Add(new Tamgiac(points[i], points[j], points[k]));
                                }
                        foreach (Tamgiac tamgiac in ListTamGiac)
                        {
                            if (Math.Round(tamgiac.DienTich(), 7) == Math.Round(DienTichGiaoNhau(fullSolid, SolidTamGiac(tamgiac)), 7)
                                && CheckTrungTamGiac21(tamgiacs, tamgiac)

                                && checkGiaoTamGiac(ListLine, tamgiac.Dinh1, tamgiac.Dinh2)
                                && checkGiaoTamGiac(ListLine, tamgiac.Dinh1, tamgiac.Dinh3)
                                && checkGiaoTamGiac(ListLine, tamgiac.Dinh2, tamgiac.Dinh3)
                                )
                            {
                                TongDienTich -= Math.Round(tamgiac.DienTich(), 7);
                                tamgiacs.Add(tamgiac);
                                Line l1 = Line.CreateBound(tamgiac.Dinh1, tamgiac.Dinh2);
                                Line l2 = Line.CreateBound(tamgiac.Dinh2, tamgiac.Dinh3);
                                Line l3 = Line.CreateBound(tamgiac.Dinh3, tamgiac.Dinh1);
                                ListLine.Add(l1);
                                ListLine.Add(l2);
                                ListLine.Add(l3);
                            }
                            if (TongDienTich == 0)
                                break;
                        }
                    }
                    TamGiacCompare tamgiacCompare = new TamGiacCompare();
                    tamgiacs.Sort(tamgiacCompare);
                    //LoadFamily
                    //Dùng đường dẫn tương đối
                    string relativePath = "Family/TCエリア・三角寸法(面積用).rfa";
                    string absolutePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePath);
                    LoadFamily(doc, relativePath);
                    //dùng đường dẫn tuyệt đối
                    //LoadFamily(doc, "D:/ThucTap/Revit/ToDuc_26102023/TCエリア・三角寸法(面積用).rfa");
                    DatFamily(doc, tamgiacs);
                    transg.Assimilate();
                }
            }
            catch (Exception ex)
            {
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        public void LoadFamily(Document doc, string fileName)
        {
            Family family = null;
            using (Transaction trans = new Transaction(doc, "Load_Family"))
            {
                trans.Start();
                doc.LoadFamily(fileName, out family);
                trans.Commit();
            }
        }
        public void DatFamily(Document doc, List<Tamgiac> tamgiacs)
        {
            FamilySymbol symbol = new FilteredElementCollector(doc)
                                    .OfClass(typeof(FamilySymbol))
                                    .WhereElementIsElementType()
                                    .Cast<FamilySymbol>()
                                    .FirstOrDefault(x => x.Name == "ラベル1.0mm");
            int so = 0;
            foreach (Tamgiac tamgiac in tamgiacs)
            {
                so++;
                using (Transaction trans = new Transaction(doc, "Family"))
                {
                    trans.Start();
                    if (!symbol.IsActive)
                    {
                        symbol.Activate();
                    }
                    //set parameter
                    FamilyInstance tam = doc.Create.NewFamilyInstance(tamgiac.TrungDiemCanhDaiNhat(), symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    Parameter DaiDay = tam.LookupParameter("エリア・幅");
                    Parameter ChieuCao = tam.LookupParameter("エリア・長さ");
                    Parameter DinhChieu = tam.LookupParameter("エリア・頂点");
                    Parameter Sott = tam.LookupParameter("エリア名称");
                    DaiDay.Set(tamgiac.DoDaiDay());
                    ChieuCao.Set(tamgiac.DienTich() / tamgiac.DoDaiDay() * 2);
                    Sott.Set(so.ToString());
                    //quay dung vi tri
                    XYZ point1 = (tam.Location as LocationPoint).Point;
                    XYZ point2 = new XYZ(point1.X, point1.Y, 10.0);
                    Line axis = Line.CreateBound(point1, point2);
                    //tinh goc lech
                    XYZ vector1 = new XYZ(100, 0, 0) - new XYZ(-100, 0, 0);
                    XYZ vector2;
                    if (tamgiac.Dinh2.X > tamgiac.Dinh3.X)
                    {
                        vector2 = tamgiac.Dinh2 - tamgiac.Dinh3;
                    }
                    else if (tamgiac.Dinh2.X == tamgiac.Dinh3.X && tamgiac.Dinh2.Y > tamgiac.Dinh3.Y)
                    {
                        vector2 = tamgiac.Dinh2 - tamgiac.Dinh3;
                    }
                    else
                        vector2 = tamgiac.Dinh3 - tamgiac.Dinh2;
                    double angle = vector1.AngleTo(vector2);
                    if (tamgiac.Dinh1.Y < tamgiac.ChanDuongCao().Y)
                    {
                        DinhChieu.Set(Math.Sqrt(tamgiac.CanhHuyen() * tamgiac.CanhHuyen()
                                    - (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2) * (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2)));
                        if (tamgiac.Dinh1.X < tamgiac.ChanDuongCao().X)
                            ElementTransformUtils.RotateElement(doc, tam.Id, axis, -angle);
                        else
                            ElementTransformUtils.RotateElement(doc, tam.Id, axis, angle);
                    }
                    else if (tamgiac.Dinh1.Y > tamgiac.ChanDuongCao().Y)
                    {
                        DinhChieu.Set(tamgiac.Dinh2.DistanceTo(tamgiac.Dinh3) - (Math.Sqrt(tamgiac.CanhHuyen() * tamgiac.CanhHuyen()
                                    - (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2) * (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2))));
                        if (tamgiac.Dinh1.X < tamgiac.ChanDuongCao().X)
                            ElementTransformUtils.RotateElement(doc, tam.Id, axis, angle + Math.PI);
                        else
                            ElementTransformUtils.RotateElement(doc, tam.Id, axis, -angle + Math.PI);
                    }
                    else
                    {
                        if (tamgiac.Dinh1.X > tamgiac.ChanDuongCao().X)
                        {
                            DinhChieu.Set(Math.Sqrt(tamgiac.CanhHuyen() * tamgiac.CanhHuyen()
                                    - (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2) * (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2)));
                            ElementTransformUtils.RotateElement(doc, tam.Id, axis, angle);
                        }
                        else
                        {
                            DinhChieu.Set(tamgiac.Dinh2.DistanceTo(tamgiac.Dinh3) - (Math.Sqrt(tamgiac.CanhHuyen() * tamgiac.CanhHuyen()
                                    - (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2) * (tamgiac.DienTich() / tamgiac.DoDaiDay() * 2))));
                            ElementTransformUtils.RotateElement(doc, tam.Id, axis, -angle);
                        }
                    }
                    ElementTransformUtils.RotateElement(doc, tam.Id, axis, 0);
                    ElementTransformUtils.MoveElement(doc, tam.Id, tamgiac.TrungDiemCanhDaiNhat() - (tam.Location as LocationPoint).Point);
                    trans.Commit();
                }
            }
        }
        /// <summary>
        /// Khoang cach tu diem den duong
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public double KC(ModelLine line, XYZ point)
        {
            XYZ p1 = line.GeometryCurve.GetEndPoint(0);
            XYZ p2 = line.GeometryCurve.GetEndPoint(1);
            Line line1 = Line.CreateBound(p1, p2);
            XYZ point1 = line1.Project(point).XYZPoint;
            return Math.Sqrt((point.X - point1.X) * (point.X - point1.X) + (point.Y - point1.Y) * (point.Y - point1.Y));
        }
        public bool CheckTamGiac(XYZ d1, XYZ d2, XYZ d3)
        {
            double k1 = d1.DistanceTo(d2);
            double k2 = d1.DistanceTo(d3);
            double k3 = d3.DistanceTo(d2);
            if (k1 + k2 > k3 &&
                k1 + k3 > k2 &&
                k2 + k3 > k1
                )
                return true;
            else
                return false;
        }
        public Solid SolidTamGiac(Tamgiac tamgiac)
        {

            Line line1 = Line.CreateBound(tamgiac.Dinh1, tamgiac.Dinh2);
            Line line2 = Line.CreateBound(tamgiac.Dinh2, tamgiac.Dinh3);
            Line line3 = Line.CreateBound(tamgiac.Dinh3, tamgiac.Dinh1);
            Solid solid;
            CurveLoop curveLoop = new CurveLoop();
            curveLoop.Append(line1);
            curveLoop.Append(line2);
            curveLoop.Append(line3);
            solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { curveLoop }, new XYZ(0, 0, 10), 10);
            return solid;
        }
        public double DienTichGiaoNhau(Solid A, Solid B)
        {
            Solid c = BooleanOperationsUtils.ExecuteBooleanOperation(A, B, BooleanOperationsType.Intersect);
            Face face = null;
            foreach (Face f in c.Faces)
            {
                XYZ normal = f.ComputeNormal(new UV(0, 0));
                if (normal.IsAlmostEqualTo(new XYZ(0, 0, 1)))
                {
                    face = f;
                    break;
                }
            }
            return (face != null) ? face.Area : 0;
        }

        //Kiểm tra 2 đường thẳng trùng nhau
        public bool CheckTrungDT(XYZ p1, XYZ p2, XYZ p3, XYZ p4)
        {
            if ((equalPoints(p1, p3) && equalPoints(p2, p4)) || (equalPoints(p1, p4) && equalPoints(p2, p3)))
                return true;
            return false;
        }
        //Kiểm tra 2 tam giác trùng nhau
        public bool CheckTrungTG(List<Line> lines, Tamgiac tg)
        {
            XYZ p1 = tg.Dinh1;
            XYZ p2 = tg.Dinh2;
            XYZ p3 = tg.Dinh3;
            int d1 = 0, d2 = 0, d3 = 0;
            foreach (Line line in lines)
            {
                XYZ p5 = line.GetEndPoint(0);
                XYZ p6 = line.GetEndPoint(1);
                if (CheckTrungDT(p1, p2, p5, p6))
                    d1++;
                if (CheckTrungDT(p1, p3, p5, p6))
                    d2++;
                if (CheckTrungDT(p2, p3, p5, p6))
                    d3++;
            }
            if (d1 > 0 && d2 > 0 && d3 > 0)
                return false;
            return true;
        }
        public bool CheckTrungTamGiac2(Tamgiac tg1, Tamgiac tg2)
        {
            XYZ p1 = tg1.Dinh1, p2 = tg1.Dinh2, p3 = tg1.Dinh3;
            XYZ d1 = tg2.Dinh1, d2 = tg2.Dinh2, d3 = tg2.Dinh3;
            if ((equalPoints(p1, d1) && equalPoints(p2, d2) && equalPoints(p3, d3))
                || (equalPoints(p1, d1) && equalPoints(p2, d3) && equalPoints(p3, d2))
                || (equalPoints(p1, d2) && equalPoints(p2, d1) && equalPoints(p3, d3))
                || (equalPoints(p1, d2) && equalPoints(p2, d3) && equalPoints(p3, d1))
                || (equalPoints(p1, d3) && equalPoints(p2, d1) && equalPoints(p3, d2))
                || (equalPoints(p1, d3) && equalPoints(p2, d2) && equalPoints(p3, d1))
                )
                return true;
            return false;
        }
        public bool CheckTrungTamGiac21(List<Tamgiac> tamgiacs, Tamgiac tg)
        {
            foreach (Tamgiac tamgiac in tamgiacs)
            {
                if (CheckTrungTamGiac2(tg, tamgiac) == true)
                    return false;
            }
            return true;
        }
        //kiem tra 2 duong thang cat nhau
        public bool checkGiao(ModelLine line1, XYZ p3, XYZ p4)
        {
            XYZ p1 = line1.GeometryCurve.GetEndPoint(0);
            XYZ p2 = line1.GeometryCurve.GetEndPoint(1);
            //XYZ p3 = line2.GeometryCurve.GetEndPoint(0);
            // XYZ p4 = line2.GeometryCurve.GetEndPoint(1);
            //TaskDialog.Show("revit", p1.ToString() + "\n" + p2.ToString() + "\n" + p3.ToString() + "\n" + p4.ToString());
            if (equalPoints(p1, p3) || equalPoints(p1, p4) || equalPoints(p2, p3) || equalPoints(p2, p4))
            {
                return false;
            }
            else
            {
                double denominator = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
                double numerator1 = (p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X);
                double numerator2 = (p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X);
                denominator = Math.Round(denominator, 5);
                numerator1 = Math.Round(numerator1, 5);
                numerator2 = Math.Round(numerator2, 5);
                double t = numerator1 / denominator;
                double u = -numerator2 / denominator;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    // Hai đường thẳng giao nhau
                    return true;
                }
                else return false;
            }
        }
        public bool checkGiaoLine(Line line, XYZ p3, XYZ p4)
        {
            XYZ p1 = line.GetEndPoint(0);
            XYZ p2 = line.GetEndPoint(1);
            if (equalPoints(p1, p3) || equalPoints(p1, p4) || equalPoints(p2, p3) || equalPoints(p2, p4))
            {
                return false;
            }
            else
            {
                double denominator = (p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X);
                double numerator1 = (p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X);
                double numerator2 = (p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X);
                denominator = Math.Round(denominator, 5);
                numerator1 = Math.Round(numerator1, 5);
                numerator2 = Math.Round(numerator2, 5);
                double t = numerator1 / denominator;
                double u = -numerator2 / denominator;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool checkGiaoTamGiac(List<Line> listLine, XYZ p3, XYZ p4)
        {
            foreach (Line line in listLine)
            {
                if (checkGiaoLine(line, p3, p4))
                {
                    return false;
                }

            }
            return true;
        }
        public bool equalPoints(XYZ a, XYZ b)
        {
            if ((Math.Round(a.X, 5) == Math.Round(b.X, 5)) && (Math.Round(a.Y, 5) == Math.Round(b.Y, 5)) && (Math.Round(a.Z, 5) == Math.Round(b.Z, 5)))
                return true;
            return false;
        }
        /// <summary>
        /// Kiểm tra xem tam giác định tạo có cắt model line nào không
        /// </summary>
        /// <param name="lines">List model lines</param>
        /// <param name="point1">Điểm đầu mút của đoạn thẳng</param>
        /// <param name="point2">Điểm đầu mút còn lại của đoạn thẳng</param>
        /// <returns></returns>
        public bool checkGiaoNhau(List<ModelLine> lines, XYZ point1, XYZ point2)
        {
            foreach (ModelLine line in lines)
            {
                if (checkGiao(line, point1, point2))
                {
                    return false;
                }
            }
            return true;
        }
        //Kiem tra tam giac dinh tao co diem nam trong khong
        public bool checkPointInTriangle(List<Tamgiac> tamgiacs, XYZ point)
        {
            foreach (Tamgiac tg in tamgiacs)
            {
                if (tg.PointInTriangle(point))
                    return false;
            }
            return true;
        }

    }
}
