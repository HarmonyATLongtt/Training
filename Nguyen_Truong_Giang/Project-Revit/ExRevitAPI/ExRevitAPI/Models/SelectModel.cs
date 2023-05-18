using Autodesk.Revit.DB;

namespace ExRevitAPI.Models
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