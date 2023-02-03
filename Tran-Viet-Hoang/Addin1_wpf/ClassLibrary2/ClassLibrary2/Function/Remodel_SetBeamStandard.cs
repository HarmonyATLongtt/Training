using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
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
                    new Remodel_SetBeamStandard().SetTopnBotBeamStandard(doc, beam);

                    asall += "Dầm " + beam.Name + " có AsTop = " + beam.AsTopLongitudinal + " mm2, AsBot = " + beam.AsBottomLongitudinal + " mm2" + "\n";
                }
                MessageBox.Show("Done");
                TaskDialog.Show("As", asall);

                transaction.Commit();
            }
        }

        public void SetTopnBotBeamStandard(Document doc, ConcreteBeamData beam)
        {
            Rebar stirrupdiameter = new Remodel_GetElem().GetStirrupTie(doc, beam.HostRebar.Host);

            double stirrup = stirrupdiameter.LookupParameter("Bar Diameter").AsDouble();
            double cover = beam.Covers.Side;
            FamilyInstance elem = beam.HostRebar.Host;
           

            XYZ origintop = new Remodel_GetBeamStandardOrigin().TopBeamStandardOrigin(elem, cover, stirrup);
            XYZ originbot = new Remodel_GetBeamStandardOrigin().BotBeamStandardOrigin(elem, cover, stirrup);

            new Remodel_SetBeamStandard().SetSingleBeamStandard(doc, beam, origintop, beam.AsTopLongitudinal);
            new Remodel_SetBeamStandard().SetSingleBeamStandard(doc, beam, originbot, beam.AsBottomLongitudinal);
        }

        public void SetSingleBeamStandard(Document doc, ConcreteBeamData beam, XYZ origin, double As)
        {
            double elemlength = beam.Length;
            double stirrup = beam.Stirrup_Tie.Type;
            double cover = beam.Covers.Side;
            RebarSetData rebardesign = new Remodel_CaculateRebar().BeamStandard(beam, cover, stirrup, As);
            string rebartype = rebardesign.Type.ToString() + "M";

            RebarShape shape = new Remodel_GetElem().GetRebarShape(doc, "M_00");
            RebarBarType type = new Remodel_GetElem().GetRebarBarType(doc, rebartype);

            XYZ xVec = xVecBeam(beam.HostRebar.Host);
            XYZ yVec = new XYZ(0, 0, 1);
            //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
            if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
            {
                //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
                // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
                xVec = -xVec;
            }
            Rebar rebarnew = Rebar.CreateFromRebarShape(doc, shape, type, beam.HostRebar.Host, origin, xVec, yVec);

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
            rebarlength.Set(elemlength - 2 * beam.Covers.Side);

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