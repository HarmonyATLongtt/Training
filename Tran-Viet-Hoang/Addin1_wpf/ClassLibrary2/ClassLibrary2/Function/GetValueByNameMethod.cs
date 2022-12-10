using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2
{
    internal class GetValueByNameMethod
    {
       
            public dynamic GetParameterValue(Parameter parameter, bool getDisplayText)
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

            public dynamic GetParameterValueByName(Element elem, string paramName, bool getDisplayText = false)
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
