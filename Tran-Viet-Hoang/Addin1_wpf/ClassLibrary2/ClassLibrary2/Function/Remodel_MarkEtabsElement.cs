using Autodesk.Revit.DB;

namespace ClassLibrary2.Function
{
    public class Remodel_MarkEtabsElement
    {
        public void SetComment(FamilyInstance ins, string name)
        {
            Parameter cmt = ins.LookupParameter("Comments");
            cmt.Set("Etabs" + name);
        }
    }
}
