using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_GetElem
    {
        public List<Level> GetListLevels(Document doc)
        {
            List<Level> elemlist = new FilteredElementCollector(doc)
                       .WhereElementIsNotElementType()
                       .OfCategory(BuiltInCategory.OST_Levels)
                       .Cast<Level>()
                       .ToList();

            return elemlist;
        }

        public List<FamilySymbol> GetListFamilySymbols(Document doc, IEnumerable<BuiltInCategory> cats)
        {
            var intValus = cats.Cast<int>().ToList();
            List<FamilySymbol> elemlist = new FilteredElementCollector(doc)
                       .WhereElementIsElementType()
                       .OfClass(typeof(FamilySymbol))
                       .Where(x => x.Category != null)
                       .Where(x => intValus.Contains(x.Category.Id.IntegerValue))
                       .Cast<FamilySymbol>()
                       .ToList();

            return elemlist;
        }

        public RebarShape GetRebarShape(Document doc, string shapename)
        {
            RebarShape elem = new FilteredElementCollector(doc)
                       .OfClass(typeof(RebarShape))
                       .Cast<RebarShape>()
                       .First(x => x.Name == shapename);

            return elem;
        }

        public RebarBarType GetRebarBarType(Document doc, string rebartype)
        {
            RebarBarType elem = new FilteredElementCollector(doc)
                       .OfClass(typeof(RebarBarType))
                       .Cast<RebarBarType>()
                       .First(x => x.Name == rebartype);
            return elem;
        }

        public Rebar GetStirrupTie(Document doc, FamilyInstance elem)
        {
            string style = "Stirrup / Tie";
            Rebar stirruptie = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_Rebar)
                  .Cast<Rebar>()
                  .First(x => x.LookupParameter("Style").AsValueString() == style && x.GetHostId() == elem.Id);
            return stirruptie;
        }
    }
}