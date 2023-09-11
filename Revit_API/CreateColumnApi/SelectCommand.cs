using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CreateColumnApi
{
    [Transaction(TransactionMode.ReadOnly)]
    public class SelectCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Selection sel = uiDoc.Selection;
            try
            {
                Element elem = doc.GetElement(sel.PickObject(ObjectType.Element, "Pick an element..."));

                if (elem != null)
                {
                    string ans = "";
                    foreach (Parameter p in elem.Parameters)
                    {
                        ans += p.Definition.Name + ": " + p.AsValueString() + "\n";
                    }
                    TaskDialog.Show("Information", ans);
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}
