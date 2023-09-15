using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CreateColumnApi
{
    [Transaction(TransactionMode.Manual)]
    public class SetCommentsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            try
            {
                Element e = doc.GetElement(uiApp.ActiveUIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select an object..."));
                Parameter p = e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                using(var trans = new Transaction(doc, "Set Comments"))
                {
                    trans.Start();
                    p.Set("New Comments");
                    trans.Commit();

                    TaskDialog.Show("Message", "Comments just set!");
                }
            }
            catch { }
            return Result.Succeeded;
        }
    }
}
