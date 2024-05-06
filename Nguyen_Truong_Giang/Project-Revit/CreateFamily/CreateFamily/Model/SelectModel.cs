using Autodesk.Revit.DB;

namespace CreateFamily.Models
{
    internal class SelectModel
    {
        public Document Doc;

        public SelectModel(Document doc)
        {
            Doc = doc;
        }
    }
}