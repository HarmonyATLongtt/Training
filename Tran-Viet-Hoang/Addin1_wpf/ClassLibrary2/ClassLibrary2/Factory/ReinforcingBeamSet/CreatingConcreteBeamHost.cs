using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Factory.ReinforcingBeamSet
{
    public class CreatingConcreteBeamHost
    {
        public void CreateBeams(Document doc, List<ConcreteBeamData> beamDatas)
        {
            var levels = Common.GetListLevels(doc);
            var beams = new List<BuiltInCategory>() { BuiltInCategory.OST_StructuralFraming };
            var beamTypes = Common.GetListFamilySymbols(doc, beams);

            if (levels.Count > 0 && beamTypes.Count > 0)
            {
                using (Transaction trans = new Transaction(doc, "create beams"))
                {
                    trans.Start();
                    foreach (ConcreteBeamData beamData in beamDatas)
                    {
                        CreateBeam(doc, beamData, levels, beamTypes);
                    }
                    trans.Commit();
                }
            }
        }

        public void CreateBeam(Document doc,
                               ConcreteBeamData beamData,
                               List<Level> levels,
                               List<FamilySymbol> beamTypes)
        {
            var beamtype = beamTypes.FirstOrDefault(x => x.Name.Equals(beamData.Dimensions.SectionName));
            var beamlevel = levels.FirstOrDefault(x => x.Name.Equals(beamData.Level));

            if (beamtype != null && beamlevel != null)
            {
                if (!beamtype.IsActive)
                {
                    beamtype.Activate();
                }
                Curve beamLine = Line.CreateBound(beamData.StartPoint.Point, beamData.EndPoint.Point);
                FamilyInstance beamnew = doc.Create.NewFamilyInstance(beamLine, beamtype, beamlevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                //mỗi khi 1 beam mới đc tạo ra thì gán ngay rebar cover của etabs mình tạo cho beam đó
                SetRebarCover(doc, beamnew, beamData);
                SetComment(beamnew, beamData.Name);

                Parameter elemlength = beamnew.LookupParameter("Length");
                beamData.Length = elemlength.AsDouble();
                beamData.Host = beamnew;

                LocationCurve locCur = beamnew.Location as LocationCurve;
                Line locline = locCur.Curve as Line;
                beamData.drawdirection = locline.Direction; // để lấy được chiều vẽ của dầm
            }
        }

        public void SetComment(FamilyInstance ins, string name)
        {
            Parameter cmt = ins.LookupParameter("Comments");
            cmt.Set("Etabs" + name);
        }

        public void SetRebarCover(Document doc, FamilyInstance fam, ConcreteBeamData beam)
        {
            var rebarcover = new FilteredElementCollector(doc)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_CoverType);

            Parameter Bottomcover = fam.LookupParameter("Rebar Cover - Bottom Face");
            Parameter Topcover = fam.LookupParameter("Rebar Cover - Top Face");

            string newnamebot = "Etabs rebar botcover value";
            string newnametop = "Etabs rebar topcover value";

            if (rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)) != null)
            {
                Parameter existlength = rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)).LookupParameter("Length");
                existlength.Set(beam.Covers.Top);
                Topcover.Set(rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)).Id);
            }
            else
            {
                RebarCoverType topp = CreateRebarCover(doc, newnametop, beam.Covers.Top);
                Topcover.Set(topp.Id);
            }

            if (rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)) != null)
            {
                Parameter existlength = rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)).LookupParameter("Length");
                existlength.Set(beam.Covers.Bottom);
                Bottomcover.Set(rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)).Id);
            }
            else
            {
                RebarCoverType bott = CreateRebarCover(doc, newnamebot, beam.Covers.Bottom);
                Bottomcover.Set(bott.Id);
            }
        }

        public RebarCoverType CreateRebarCover(Document doc, string name, double coverDistance)
        {
            RebarCoverType rebarcovernew = RebarCoverType.Create(doc, name, coverDistance);
            return rebarcovernew;
        }
    }
}