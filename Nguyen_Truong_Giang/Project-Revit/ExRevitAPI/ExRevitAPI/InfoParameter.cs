using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace ExRevitAPI
{
    [Transaction(TransactionMode.Manual)]
    class InfoParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            Reference r = uidoc.Selection.PickObject(ObjectType.Element);
            Element element = uidoc.Document.GetElement(r);


            ParameterSet paraset = element.Parameters;

            foreach (Parameter parameter in paraset)
            {
                string parameterName = parameter.Definition.Name;
                string parameterValue = parameter.AsValueString();
                string paraDataType = parameter.Definition.ParameterType.ToString();

                // In ra thông tin parameter
                TaskDialog.Show("Parameter info", 
                    "Parameter Name: " + parameterName + "\n"
                    + "Parameter Value: " + parameterValue + "\n"
                    + "Data Type: " + paraDataType);
            }


            return Result.Succeeded;
        }
    }
}
