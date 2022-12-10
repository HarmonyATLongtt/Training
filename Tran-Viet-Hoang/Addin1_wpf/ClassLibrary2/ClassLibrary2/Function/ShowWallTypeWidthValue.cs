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
   
            ElementCategoryFilter filterWall = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            IList<Element> wallelems = new FilteredElementCollector(doc).WherePasses(filterWall).ToElements();
            string walldialog = "";
            foreach (Element specificwall in wallelems)
            {
                if (null == specificwall.GetTypeId())
                {
                    TaskDialog.Show("Revit", "Not found");
                }
                else
                {
                    Parameter parameter = specificwall.LookupParameter("Width");
                    if (parameter != null)
                    {
                        double? value = new GetValueByNameMethod().GetParameterValueByName(specificwall, "Width");
                        walldialog += specificwall.Name + " has widthvalue is: " + value * 304.8 + " mm" + "\n";
                    }
                }
            }
            TaskDialog.Show("Revit", walldialog);
            return Result.Succeeded;
        }
    }
}
