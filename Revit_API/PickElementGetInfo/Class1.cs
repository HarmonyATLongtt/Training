using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace PickElementGetInfo
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            Reference pickedref = null;

            Selection sel = uiApp.ActiveUIDocument.Selection;

            pickedref = sel.PickObject(ObjectType.Element, "Select object you want to get information");

            Element elem = doc.GetElement(pickedref);

            string ans = "";

            foreach(Parameter i in elem.Parameters)
            {
                ans += i.Definition.Name + ": " + i.AsValueString() + "\n";
            }

            TaskDialog.Show("Information", ans);
            return Result.Succeeded;
        }
    }
}
