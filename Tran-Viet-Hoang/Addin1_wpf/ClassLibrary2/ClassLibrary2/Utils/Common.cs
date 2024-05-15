using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Utils
{
    public static class Common
    {
        public static double Max(double num1, double num2)
        {
            double max = 0;
            if (num1 <= num2)
            {
                max = num2;
            }
            else
            {
                max = num1;
            }
            return max;
        }

        public static List<Level> GetListLevels(Document doc)
        {
            List<Level> elemlist = new FilteredElementCollector(doc)
                       .WhereElementIsNotElementType()
                       .OfCategory(BuiltInCategory.OST_Levels)
                       .Cast<Level>()
                       .ToList();

            return elemlist;
        }

        public static List<FamilySymbol> GetListFamilySymbols(Document doc, IEnumerable<BuiltInCategory> cats)
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

        public static Rebar GetStirrupTie(Document doc, FamilyInstance elem)
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