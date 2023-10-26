using Autodesk.Revit.DB;

namespace RevitAPI_B1.Utils
{
    public class FamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true; // Ghi đè các giá trị tham số
            return true; // Tiếp tục tải gia đình
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family; // Tải gia đình từ tệp tin gia đình
            overwriteParameterValues = true; // Ghi đè các giá trị tham số
            return true; // Tiếp tục tải gia đình
        }
    }
}
