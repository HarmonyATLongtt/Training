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


namespace GetBeamWidth
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

            FilteredElementCollector collection = new FilteredElementCollector(doc);
            ICollection<Element> beams = collection.OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<Element>().ToList();

            string mess = "Các độ rộng của dầm là: " + "\n";
            foreach (var beam in beams)
            {             
                if(beam!= null)
                {
                  Parameter parameter =  beam.LookupParameter("b");
                    if (parameter != null)
                    {
                       double? value = GetParameterValueByName(beam, "b");
                        mess +=  value*304.8 + "\n";
                    }
                }
            }
            TaskDialog.Show("Revit", mess);
            return Result.Succeeded;
        }

        public  dynamic GetParameterValue(Parameter parameter, bool getDisplayText)
        {
            if (parameter != null && parameter.HasValue)
            {
                if (getDisplayText)
                    return parameter.AsValueString();

                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        return parameter.AsDouble();

                    case StorageType.ElementId:
                        return parameter.AsElementId();

                    case StorageType.Integer:
                        return parameter.AsInteger();

                    case StorageType.String:
                        return parameter.AsString();
                }
            }
            return null;
        }

        public  dynamic GetParameterValueByName(Element elem, string paramName, bool getDisplayText = false)
        {
            if (elem != null)
            {
                Parameter parameter = elem.LookupParameter(paramName);
                return GetParameterValue(parameter, getDisplayText);
            }
            return null;
        }
    }
}
