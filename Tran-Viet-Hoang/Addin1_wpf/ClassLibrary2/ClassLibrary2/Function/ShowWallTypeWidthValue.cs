using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary2;
using System.Collections.Generic;

namespace ShowWallTypeWidthValue
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class AppRevit : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;         
            Document doc = uidoc.Document;
   
            ElementCategoryFilter filterW = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            IList<Element> filterlogic = new FilteredElementCollector(doc).WherePasses(filterW).ToElements();
            string mess = "";
            foreach (Element eachwall in filterlogic)
            {
                if (null == eachwall.GetTypeId())
                {
                    TaskDialog.Show("Revit", "Not found");
                }
                else
                {
                    Parameter parameter = eachwall.LookupParameter("Width");
                    if (parameter != null)
                    {
                        double? value = new GetValueByNameMethod().GetParameterValueByName(eachwall, "Width");
                        mess += eachwall.Name + " has widthvalue is: " + value * 304.8 + " mm" + "\n";
                    }
                }
            }
            TaskDialog.Show("Revit", mess);
            return Result.Succeeded;
        }
    }
}
