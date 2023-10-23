using Autodesk.Revit.DB;
using System.Linq;

namespace MultiRoom.Utils
{
    public class LoadFamily
    {
        public Family GetFamily(Document doc)
        {
            Family family = null;

            family = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(fa => fa.Name.Equals("TCエリア・矩形寸法(面積用)"));

            if (family != null)
                return family;

            // Need edit direct path
            string path = "C:\\Users\\admin\\Downloads\\Documents\\Intern Document\\Task Train Work\\TCエリア・矩形寸法(面積用).rfa";

            if (!doc.LoadFamily(path, out family))
            {
                throw new System.Exception("Unable to load " + path);
            }

            return family;
        }
    }
}