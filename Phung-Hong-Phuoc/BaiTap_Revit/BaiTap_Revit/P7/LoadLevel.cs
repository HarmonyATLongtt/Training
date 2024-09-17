using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BaiTap_Revit.P7
{
    [Transaction(TransactionMode.Manual)]
    internal class LoadLevel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            UI wpfForm = new UI(doc);
            wpfForm.ShowDialog();
            return Result.Succeeded;
        }
    }
}