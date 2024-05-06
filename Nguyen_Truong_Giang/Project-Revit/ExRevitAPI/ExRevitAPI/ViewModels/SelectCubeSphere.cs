using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ExRevitAPI
{
    [Transaction(TransactionMode.Manual)]
    internal class SelectCubeSphere : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            SelectView wpfForm = new SelectView(doc);

            wpfForm.ShowDialog();

            return Result.Succeeded;
        }
    }
}