using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
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
                    new Remodel_SetBeamStandard().SetTopnBotBeamStandard(doc, beamtype, beam);

                    asall += "Dầm " + beam.Name + " có AsTop = " + beam.AsTopLongitudinal + " mm2, AsBot = " + beam.AsBottomLongitudinal + " mm2" + "\n";
                }
                MessageBox.Show("Done");
                TaskDialog.Show("As", asall);

                transaction.Commit();
            }
        }

        public void SetTopnBotBeamStandard(Document doc, FamilyInstance elem, ConcreteBeamData beam)
        {
            double stirrup = 12 / 304.8;
            double cover = 50 / 304.8;

            XYZ origintop = new Remodel_GetBeamStandardOrigin().TopBeamStandardOrigin(elem, cover, stirrup);
            XYZ originbot = new Remodel_GetBeamStandardOrigin().BotBeamStandardOrigin(elem, cover, stirrup);

            new Remodel_SetBeamStandard().SetSingleBeamStandard(doc, elem, origintop, beam.AsTopLongitudinal, stirrup, cover);
            new Remodel_SetBeamStandard().SetSingleBeamStandard(doc, elem, originbot, beam.AsBottomLongitudinal, stirrup, cover);
        }

        public void SetSingleBeamStandard(Document doc, FamilyInstance elem, XYZ origin, double As, double stirrup, double cover)
        {
            Parameter elemlength = elem.LookupParameter("Length");
            Parameter elemb = elem.Symbol.LookupParameter("b");
            Parameter elemh = elem.Symbol.LookupParameter("h");

            RebarSetData rebardesign = new Remodel_CaculateRebar().BeamStandard(elem, cover, stirrup, As);

            string rebartype = rebardesign.Type.ToString() + "M";

            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_00");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == rebartype);

            XYZ xVec = xVecBeam(elem);

            XYZ yVec = new XYZ(0, 0, 1);
            //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
            if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
            {
                //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
                // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
                xVec = -xVec;
            }
            Rebar rebarnew = Rebar.CreateFromRebarShape(doc, shape, type, elem, origin, xVec, yVec);

            Parameter rebarlength = rebarnew.LookupParameter("B");

            double oldlength = rebarlength.AsDouble(); //giữ lại giá trị length ban đầu để sau thực hiện rotate

            XYZ point1 = XYZ.Zero;

            //kiểm tra xem cấu kiện được vẽ theo phương nào để có thể lấy được trục để rotate
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                point1 = origin + XYZ.BasisX * oldlength / 2; ;
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                point1 = origin + XYZ.BasisY * oldlength / 2; ;
            }

            XYZ point2 = point1 + XYZ.BasisZ * 100;
            Line axis = Line.CreateBound(point1, point2);

            // set giá trị mới cho length cuả rebar
            rebarlength.Set(elemlength.AsDouble() - 2 * cover);

            ElementTransformUtils.RotateElement(doc, rebarnew.Id, axis, Math.PI);

            rebarnew.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(rebardesign.Number, rebardesign.Spacing / 304.8, false, true, true);
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