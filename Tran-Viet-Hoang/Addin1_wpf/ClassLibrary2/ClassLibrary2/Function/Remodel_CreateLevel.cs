using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClassLibrary2.Function
{
    public class Remodel_CreateLevel
    {
        public void CreateLevel(ExternalCommandData commandData, string uniquename, double elevation)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            var elems = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels);
            string Levelexisted = "false";

            using (var transaction = new Transaction(doc, "Set Elevation"))
            {
                foreach (var singlelevel in elems)
                {
                    Parameter para = singlelevel.LookupParameter("Name");
                    Parameter cote = singlelevel.LookupParameter("Elevation");
                    if (double.TryParse(cote.AsValueString(), out double cotelevel))
                    {
                        if (para.AsString() == uniquename)
                        {
                            transaction.Start();
                            Levelexisted = "true";
                            cote.Set(elevation);
                            transaction.Commit();
                        }
                    }
                }

                if (Levelexisted == "false")
                {
                    transaction.Start();
                    Level newlevel = Level.Create(doc, elevation);
                    Parameter paranew = newlevel.LookupParameter("Name");
                    paranew.Set(uniquename);
                    transaction.Commit();
                }
            }
        }
    }
}
