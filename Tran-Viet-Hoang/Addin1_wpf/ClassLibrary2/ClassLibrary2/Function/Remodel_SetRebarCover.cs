﻿using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_SetRebarCover
    {
        public void SetRebarCover(Document doc, FamilyInstance fam, ConcreteBeamData beam)
        {
            var rebarcover = new FilteredElementCollector(doc)
                .WhereElementIsElementType()
                .OfCategory(BuiltInCategory.OST_CoverType);

            Parameter Bottomcover = fam.LookupParameter("Rebar Cover - Bottom Face");
            string newnamebot = "Etabs rebar botcover value";

            Parameter Topcover = fam.LookupParameter("Rebar Cover - Top Face");
            string newnametop = "Etabs rebar topcover value";

            //Parameter othercover = fam.LookupParameter("Rebar Cover - Other Faces");

            if (rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)) != null)
            {
                Parameter existlength = rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)).LookupParameter("Length");
                existlength.Set(beam.TopCover);
                Topcover.Set(rebarcover.FirstOrDefault(x => x.Name.Equals(newnametop)).Id);
            }
            else
            {
                RebarCoverType topp = CreateRebarCover(doc, newnametop, beam.TopCover);
                Topcover.Set(topp.Id);
            }

            if (rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)) != null)
            {
                Parameter existlength = rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)).LookupParameter("Length");
                existlength.Set(beam.BottomCover);
                Bottomcover.Set(rebarcover.FirstOrDefault(x => x.Name.Equals(newnamebot)).Id);
            }
            else
            {
                RebarCoverType bott = CreateRebarCover(doc, newnamebot, beam.BottomCover);
                Bottomcover.Set(bott.Id);
                //othercover.Set(fifty.Id);
            }
        }

        public RebarCoverType CreateRebarCover(Document doc, string name, double coverDistance)
        {
            RebarCoverType rebarcovernew = RebarCoverType.Create(doc, name, coverDistance);
            return rebarcovernew;
        }
    }
}
