using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows;

namespace ClassLibrary2.Function
{
    public class Remodel_SetBeamStandard
    {
        //hàm trả về điểm origin để vẽ thép dọc cho dầm
        public void SetAllBeamStandard(Document doc, List<ConcreteBeamData> beams)
        {
            using (var transaction = new Transaction(doc, "Create rebar "))
            {
                transaction.Start();
                string asall = string.Empty;
                foreach (var beam in beams)
                {
                    string cmt = "Etabs" + beam.Name;
                    var beamtype = new FilteredElementCollector(doc)
                      .WhereElementIsNotElementType()
                      .OfCategory(BuiltInCategory.OST_StructuralFraming)
                      .Cast<FamilyInstance>()
                      .First(x => x.LookupParameter("Comments").AsString() == cmt);
                    SetBeamStandard(doc, beamtype, beam);
                    asall += "Dầm " + beam.Name + " có AsTop = " + beam.AsTopLongitudinal + " mm2, AsBot = " + beam.AsBottomLongitudinal + " mm2" + "\n";
                }
                MessageBox.Show("Done");
                TaskDialog.Show("As", asall);

                transaction.Commit();
            }

        }
        public void SetBeamStandard(Document doc, FamilyInstance elem, ConcreteBeamData beam)
        {
            double stirrup = 12 / 304.8;
            double cover = 50 / 304.8;
            Parameter elemlength = elem.LookupParameter("Length");
            Parameter elemb = elem.Symbol.LookupParameter("b");
            Parameter elemh = elem.Symbol.LookupParameter("h");

            RebarSetData designBot = new Remodel_CaculateRebar().BeamStandard(elem, cover, stirrup, beam.AsBottomLongitudinal);
            RebarSetData designTop = new Remodel_CaculateRebar().BeamStandard(elem, cover, stirrup, beam.AsTopLongitudinal);

            string botrebartype = designBot.Type.ToString() + "M";
            string toprebartype = designTop.Type.ToString() + "M";

            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_00");

            RebarBarType bottype = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == botrebartype);
            RebarBarType toptype = new FilteredElementCollector(doc)
               .OfClass(typeof(RebarBarType))
               .Cast<RebarBarType>()
               .First(x => x.Name == toprebartype);

            XYZ xVec = xVecBeam(elem);
            XYZ origin1bot = new Remodel_GetBeamStandardOrigin().BotBeamStandardOrigin(elem, cover, stirrup);
            XYZ origin1top = new Remodel_GetBeamStandardOrigin().TopBeamStandardOrigin(elem, cover, stirrup);

            XYZ yVec = new XYZ(0, 0, 1);
            //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
            if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
            {
                //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
                // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
                xVec = -xVec;
            }
            Rebar bottomrebar = Rebar.CreateFromRebarShape(doc, shape, bottype, elem, origin1bot, xVec, yVec);
            Rebar toprebar = Rebar.CreateFromRebarShape(doc, shape, toptype, elem, origin1top, xVec, yVec);

            Parameter botrebarlength = bottomrebar.LookupParameter("B");
            Parameter toprebarlength = toprebar.LookupParameter("B");

            double oldlength = botrebarlength.AsDouble(); //giữ lại giá trị length ban đầu để sau thực hiện rotate

            XYZ point1 = XYZ.Zero;

            //kiểm tra xem cấu kiện được vẽ theo phương nào để có thể lấy được trục để rotate
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                point1 = origin1bot + XYZ.BasisX * oldlength / 2; ;
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                point1 = origin1bot + XYZ.BasisY * oldlength / 2; ;
            }

            XYZ point2 = point1 + XYZ.BasisZ * 100;
            Line axis = Line.CreateBound(point1, point2);

            // set giá trị mới cho length cuả rebar
            botrebarlength.Set(elemlength.AsDouble() - 2 * cover);
            toprebarlength.Set(elemlength.AsDouble() - 2 * cover);

            ElementTransformUtils.RotateElement(doc, bottomrebar.Id, axis, Math.PI);
            ElementTransformUtils.RotateElement(doc, toprebar.Id, axis, Math.PI);

            bottomrebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(designBot.Number, designBot.Spacing / 304.8, false, true, true);
            toprebar.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(designTop.Number, designTop.Spacing / 304.8, false, true, true);
        }

        public XYZ xVecBeam(FamilyInstance elem)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = elem.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            XYZ xVec = locline.Direction;
            return xVec;
        }
    }
}
