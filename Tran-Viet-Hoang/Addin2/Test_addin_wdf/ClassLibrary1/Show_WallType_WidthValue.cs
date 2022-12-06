using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows;
using System.Data;
using Autodesk.Revit.DB.Structure;
using ClassLibrary1;

namespace Show_WallType_WidthValue
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Collect cấu kiện có vật liệu là bê tông         
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
                    if(parameter != null)
                    {
                        double? value = new GetValueByNameMethod().GetParameterValueByName(eachwall, "Width");
                        mess += eachwall.Name + " has widthvalue is: "+value*304.8 +" mm"+ "\n";
                    }                                      
                }          
            }
            TaskDialog.Show("Revit", mess);                                 
            return Result.Succeeded;
        }       
    }
}
