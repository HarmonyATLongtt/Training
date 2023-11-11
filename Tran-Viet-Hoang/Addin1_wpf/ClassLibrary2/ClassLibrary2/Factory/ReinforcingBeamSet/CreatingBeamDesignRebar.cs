using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Data.FrameData;
using ClassLibrary2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Factory.ReinforcingBeamSet
{
    public class CreatingBeamDesignRebar
    {
        // hàm tạo 1 stirrup cho nhiều dầm và set lại giá trị stirrup đó sao cho phù hợp với kích thước dầm
        public void CreateBeamRebar(Document doc, List<ConcreteBeamData> beams)
        {
            var rebarFactory = new RebarSet.RebarSet(doc);
            foreach (var beam in beams)
                beam.Rebars = rebarFactory.CalculateBeamRebar(beam);

            using (Transaction trans = new Transaction(doc, "create beam stirrup"))
            {
                trans.Start();
                foreach (var beametabs in beams)
                {
                    Stirrupbeambefore(beametabs, doc);
                }
                MoveStirrup(doc, beams);
                SetAllBeamStandard(doc, beams);
                trans.Commit();
            }
        }

        //Hàm tạo 1 stirrup ban đầu cho 1 dầm
        public Rebar Stirrupbeambefore(ConcreteBeamData beametabs, Document doc)
        {
            RebarSetData stirrup = new RebarSet.RebarSet(doc).CalculateBeamRebar(beametabs).FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);

            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            XYZ xVec = beametabs.drawdirection;
            //khai báo giá trị other cover, để xác định chính xác length của thép

            XYZ yVec = new XYZ(0, 0, 1);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                xVec = new XYZ(0, 1, 0);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                xVec = new XYZ(1, 0, 0);
            }

            Rebar rebar = Rebar.CreateFromRebarShape(doc,
                stirrup.ShapeData.Shape,
                stirrup.Rebartype,
                beametabs.Host,
                stirrup.LocationData.RebarOrigin, xVec, yVec);
            stirrup.Rebars = rebar;

            foreach (var para in stirrup.ShapeData.Segments)
            {
                Parameter dim = rebar.LookupParameter(para.Key);
                dim.Set(para.Value);
            }

            return rebar;
        }

        public void MoveStirrup(Document doc, List<ConcreteBeamData> beams)
        {
            double cover = new ConcreteHostData().Covers.Side;

            //dầm
            foreach (var beam in beams)
            {
                var Ins = Common.GetStirrupTie(doc, beam.Host as FamilyInstance);
                RebarSetData stirrup = beam.Rebars.FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);
                XYZ origin1st = stirrup.LocationData.RebarOrigin;

                BoundingBoxXYZ boundingboxnew = Ins.get_BoundingBox(null);
                XYZ origin2nd = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                XYZ vect = origin1st - origin2nd;

                ElementTransformUtils.MoveElement(doc, Ins.Id, vect);
                Ins.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrup.LayoutData.Spacing, beam.Length, true, true, false);
            }
        }

        public void SetAllBeamStandard(Document doc, List<ConcreteBeamData> beams)
        {
            string asall = string.Empty;
            foreach (var beam in beams)
            {
                SetTopnBotBeamStandard(doc, beam);

                asall += "Dầm " + beam.Name + " có AsTop = " + beam.Reinforcing.AsTop / 0.00001076391 + " mm2, AsBot = " + beam.Reinforcing.AsBot / 0.00001076391 + " mm2" + "\n";
            }

            //var find = beams.FirstOrDefault(x => x.Name == "48");
            //var thepdoc = find.Rebars.Where(x => x.Style == RebarStyle.Standard).ToList();
            //asall = "Dầm " + find.Name + "\n";
            //foreach (var thep in thepdoc)
            //{
            //    asall += "Thép phi: "+thep.Rebartype.BarDiameter * 304.8 + "\n" +
            //            "Min host boundingbox: "+(find.Host as FamilyInstance).get_BoundingBox(null).Min.ToString() + "\n" +
            //            "Rebar origin: " +thep.LocationData.RebarOrigin + "\n";
            //}

            //MessageBox.Show("Done");
            //TaskDialog.Show("As", asall);
        }

        public void SetTopnBotBeamStandard(Document doc, ConcreteBeamData beam)
        {
            List<RebarSetData> datas = new RebarSet.RebarSet(doc).CalculateBeamRebar(beam);
            foreach (var data in datas)
            {
                if (data != null && data.Style == RebarStyle.Standard)
                {
                    SetSingleBeamStandard(doc, beam, data);
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

            // point 2 chỉ khác point 1 ở chỉ số Z
            XYZ point2 = point1 + XYZ.BasisZ * 100;
            // đường thẳng tạo từ point 1 và point 2 là đường thẳng song song với trục Z
            Line axis = Line.CreateBound(point1, point2);

            // set giá trị mới cho length cuả rebar
            rebarlength.Set(beam.Length - 2 * beam.Covers.Side);

            ElementTransformUtils.RotateElement(doc, rebarnew.Id, axis, Math.PI);
            rebarnew.GetShapeDrivenAccessor().SetLayoutAsNumberWithSpacing(beamstandard.LayoutData.Number, beamstandard.LayoutData.Spacing, false, true, true);
        }

        public XYZ xVecBeam(FamilyInstance elem)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            LocationCurve locCur = elem.Location as LocationCurve;
            Line locline = locCur.Curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            XYZ xVec = locline.Direction;
            return xVec;
        }
    }
}