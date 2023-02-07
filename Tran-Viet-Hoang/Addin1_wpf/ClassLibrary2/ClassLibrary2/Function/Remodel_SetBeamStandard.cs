using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using ClassLibrary2.Factory.RebarSet;
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

                    asall += "Dầm " + beam.Name + " có AsTop = " + beam.Reinforcing.AsTop / 0.00001076391 + " mm2, AsBot = " + beam.Reinforcing.AsBot / 0.00001076391 + " mm2" + "\n";
                }
                MessageBox.Show("Done");
                TaskDialog.Show("As", asall);

                transaction.Commit();
            }
        }

        public void SetTopnBotBeamStandard(Document doc, ConcreteBeamData beam)
        {
            List<RebarSetData> datas = new RebarSet(doc).CalculateBeamRebar(beam);
            foreach (var data in datas)
            {
                if (data != null && data.Style == RebarStyle.Standard)
                {
                    new Remodel_SetBeamStandard().SetSingleBeamStandard(doc, beam, data);
                }
            }
        }

        public void SetSingleBeamStandard(Document doc, ConcreteBeamData beam, RebarSetData beamstandard)
        {
            XYZ origin = beamstandard.LocationData.RebarOrigin;

            RebarShape shape = beamstandard.ShapeData.Shape;
            RebarBarType type = beamstandard.Rebartype;

            XYZ xVec = xVecBeam(beam.Host as FamilyInstance);
            XYZ yVec = new XYZ(0, 0, 1);
            //nếu Bounding box ngược chiều với hướng vẽ element thì thép sẽ bị bay ra ngoài, vậy nên kiểm tra nếu ngược chiều thì gán chiều vẽ thép bằng ngược chiều của chiều vẽ dầm
            if (Math.Round(xVec.X, 4) < 0 || Math.Round(xVec.Y, 4) < 0)
            {
                //dù giá trị vector có thể là (1.0000; 0,00000; 0,00000) nhưng riêng giá trị Y có thể là giá trị : -3.0723465E-15 - giá trị rất gần với 0 nhưng vẫn là < 0)
                // nên phải làm tròn số thập phân của X hoặc Y của vector để lệnh if kiểm tra được như ý muốn
                xVec = -xVec;
            }
            Rebar rebarnew = Rebar.CreateFromRebarShape(doc, shape, type, beam.Host, origin, xVec, yVec);

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
            rebarlength.Set(beam.Length - 2 * beam.Covers.Side);

            ElementTransformUtils.RotateElement(doc, rebarnew.Id, axis, Math.PI);
            rebarnew.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(beamstandard.LayoutData.Number, beamstandard.LayoutData.Spacing, false, true, true);
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