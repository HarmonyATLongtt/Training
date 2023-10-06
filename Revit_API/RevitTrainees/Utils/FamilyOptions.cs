using Autodesk.Revit.DB;

namespace RevitTrainees.Utils
{
    public class FamilyOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = default;
            overwriteParameterValues = true;
            return true;
        }
    }
}