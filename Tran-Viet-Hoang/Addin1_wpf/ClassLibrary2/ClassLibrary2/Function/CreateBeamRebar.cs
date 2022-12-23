using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using System;
using System.Linq;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace CreateBeamRebar
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Reference beam = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick");
            Element elem = doc.GetElement(beam);
            FamilyInstance eleminstance = doc.GetElement(beam) as FamilyInstance;

            Parameter elemlength = elem.LookupParameter("Length");
            Parameter elemb = eleminstance.Symbol.LookupParameter("b");
            Parameter elemh = eleminstance.Symbol.LookupParameter("h");

            //lấy giá trị length của cấu kiện
            string elemlengthvalue = elemlength.AsValueString();
            //khai báo giá trị other cover, để xác định chính xác length của thép
            double cover = 45 / 304.8;
            double stirrup = 8 / 304.8;

            double Astinhtoan = 480.596; //Ví dụ 1 giá trị As của Etabs trả về : mm2
            double Asmin = elemb.AsDouble() * 304.8 * elemh.AsDouble() * 304.8 * 0.05 / 100;

            RebarSetData designrebar = RebarBeamCaculation(eleminstance, cover, stirrup, Astinhtoan);

            MessageBox.Show("Cấu kiện dầm có b: " + elemb.AsValueString() + "mm & h: " + elemh.AsValueString() + "mm" + "\n" +
                "As tính toán là: " + Astinhtoan + " mm2" + "\n" +
                "As tối thiểu là (0.05%): " + Asmin + " mm2" + "\n" +
                "As bố trí : " + Math.Round(designrebar.RebarCrossSectionArea, 3) + "mm2" + "\n" +
                "Với phướng án thép lớp dưới là: " + designrebar.Number + "D" + designrebar.Type + "\n" +
                "Các thanh thép cách nhau là: " + designrebar.Spacing*304.8)
                   ;
            string rebartype = designrebar.Type.ToString() + "M";
          
            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_00");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == rebartype);

            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = elem.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            XYZ yVec = new XYZ(0, 0, 1);
            XYZ xVec = locline.Direction;

            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin1 = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin1 = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Max.Y - cover - stirrup, boundingbox.Min.Z + cover + stirrup);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin1 = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Min.Y + cover + stirrup, boundingbox.Min.Z + cover + stirrup);
            }
            //XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
            //XYZ max = boundingbox.Transform.OfPoint(boundingbox.Max);

            try
            {
                using (var transaction = new Transaction(doc, "Create rebar "))
                {
                    transaction.Start();

                    //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
                    if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
                    {
                        //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
                        // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
                        xVec = -locline.Direction;
                    }
                    Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, elem, origin1, xVec, yVec);

                    Parameter rebarlength = rebar.LookupParameter("B");
                    double oldlength = rebarlength.AsDouble(); //giữ lại giá trị length ban đầu để sau thực hiện rotate

                    XYZ point1 = XYZ.Zero;

                    //kiểm tra xem cấu kiện được vẽ theo phương nào để có thể lấy được trục để rotate
                    if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
                    {
                        point1 = origin1 + XYZ.BasisX * oldlength / 2; ;
                    }
                    else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
                    {
                        point1 = origin1 + XYZ.BasisY * oldlength / 2; ;
                    }

                    XYZ point2 = point1 + XYZ.BasisZ * 100;
                    Line axis = Line.CreateBound(point1, point2);

                    // set giá trị mới cho length cuả rebar
                    rebarlength.Set(Convert.ToDouble(elemlengthvalue) / 304.8 - 2 * cover);

                    ElementTransformUtils.RotateElement(doc, rebar.Id, axis, Math.PI);

                    //rebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(designrebar.Number, designrebar.Spacing, false, true, true);

                    transaction.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        public RebarSetData RebarBeamCaculation(FamilyInstance beam, double cover, double stirrup, double Astinhtoan)
        {
            double kc = 25; //khoảng cách thông thủy tối thiểu giữa các thanh thép lớp dưới
            int[] duongkinhcautao = { 16, 18, 20, 22 };
            int[] sothanh = new int[4];


            Parameter elemb = beam.Symbol.LookupParameter("b");
            Parameter elemh = beam.Symbol.LookupParameter("h");
            double Asmin = elemb.AsDouble() * 304.8 * elemh.AsDouble() * 304.8 * 0.05 / 100; // diện tích cốt thép tối thiểu là 0,05%
            if(Asmin < Astinhtoan) { Asmin = Astinhtoan;} // chọn ra giá trị mà As thiết kế bắt buộc sẽ phải lớn hơn
            RebarSetData rebarsets = new RebarSetData();
            
            for (int i = 0; i < duongkinhcautao.Count(); i++)
            {
                //số thanh phải nhỏ hơn hoặc bằng, nên dùng hàm Floor để lấy giá trị nguyên lớn nhất và gần kết quả nhất
                sothanh[i] = Convert.ToInt32(Math.Floor((elemb.AsDouble() * 304.8 + kc - 2* (cover+stirrup) * 304.8) / (duongkinhcautao[i] + kc)));
            }
            for (int i = 0; i < sothanh.Count(); i++)
            {
                if (Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i] >= Asmin)
                {
                    RebarSetData rebarset = new RebarSetData();
                    rebarset.Number = sothanh[i];
                    rebarset.Type = duongkinhcautao[i];
                    rebarset.RebarCrossSectionArea = Math.Pow(duongkinhcautao[i], 2) * Math.PI / 4 * sothanh[i];
                    rebarset.CrossSectionWidth = elemb.AsDouble();
                    rebarset.Spacing = (rebarset.CrossSectionWidth - 2*cover - 2*stirrup - rebarset.Type/304.8) / (rebarset.Number - 1);
                    rebarsets = rebarset;
                    break;
                }
                
            }
            return rebarsets;
        }
    }
}