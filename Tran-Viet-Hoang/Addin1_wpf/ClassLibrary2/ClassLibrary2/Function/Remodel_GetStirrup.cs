using Autodesk.Revit.DB;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;

namespace ClassLibrary2.Function
{
    public class Remodel_GetStirrup
    {
        // hàm tạo list trả về các giá trị kiểu Rebar, giá trị length, b, h và boundingbox của host column
        public List<RebarSetData> ColumnStirrup(Document doc, List<ConcreteColumnData> cols)
        {
            List<RebarSetData> rebars = new List<RebarSetData>();
            foreach (var col in cols)
            {
                var stirrup = new Remodel_GetElem().GetStirrupTie(doc, col.HostRebar.HostData.Host);

                if (stirrup != null)
                {
                    RebarSetData rebar = new RebarSetData();
                    rebar.Rebar = stirrup;
                    rebar.HostData.HostLength = col.Length;
                    rebar.HostData.Host_h = col.Dimensions.h;
                    rebar.HostData.Host_b = col.Dimensions.b;
                    rebar.LocationData.BoundingBox = col.HostRebar.HostData.Host.get_BoundingBox(null);
                    rebars.Add(rebar);
                }
            }
            return rebars;
        }

        // hàm tạo list trả về các giá trị kiểu Rebar, giá trị length, b, h và boundingbox của host beam
        public List<RebarSetData> BeamStirrup(Document doc, List<ConcreteBeamData> beams)
        {
            List<RebarSetData> rebars = new List<RebarSetData>();
            foreach (var beam in beams)
            {
                var stirrup = new Remodel_GetElem().GetStirrupTie(doc, beam.HostRebar.HostData.Host);

                if (stirrup != null)
                {
                    RebarSetData rebar = new RebarSetData();
                    rebar.BeamStirrup = stirrup;
                    rebar.HostData.HostLength = beam.Length;
                    rebar.HostData.Host_h = beam.Dimensions.h;
                    rebar.HostData.Host_b = beam.Dimensions.b;
                    rebar.LocationData.RebarOrigin = FrameStirrupOrigin(beam);
                    rebars.Add(rebar);
                }
            }
            return rebars;
        }

        // Hàm lấy origin ban đầu về cho stirrup của dầm
        public XYZ FrameStirrupOrigin(ConcreteBeamData beametabs)
        {
            beametabs.HostRebar = new RebarSetData();
            beametabs.HostRebar.HostData = new RebarHostData();
            double coverbot = beametabs.Covers.Top;
            double coverside = beametabs.Covers.Side;
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y

            XYZ xVec = beametabs.drawdirection; // để lấy được chiều vẽ của dầm
            FamilyInstance ins = beametabs.Host as FamilyInstance;
            BoundingBoxXYZ boundingbox = ins.get_BoundingBox(null);
          

            XYZ origin = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                origin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Min.Y + coverside, boundingbox.Min.Z + coverbot);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                origin = new XYZ(boundingbox.Min.X + coverside, boundingbox.Max.Y - coverside, boundingbox.Min.Z + coverbot);
            }
            return origin;
        }
    }
}