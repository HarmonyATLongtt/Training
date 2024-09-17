using Autodesk.Revit.Attributes;
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BaiTap_Revit.P1
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    internal class PrintParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Lấy đối tượng được chọn
                Reference selectedRef = uidoc.Selection.PickObject(ObjectType.Element, "Please select an element");
                Element selectedElement = doc.GetElement(selectedRef);

                // In ra các tham số của đối tượng đã chọn
                TaskDialog.Show("Selected Element Parameters", GetElementParameters(selectedElement));
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private string GetElementParameters(Element element)
        {
            string parameters = string.Empty;
            foreach (Parameter param in element.Parameters)
            {
                string paramValue = param.AsValueString() ?? param.AsString();
                parameters += $"{param.Definition.Name}: {paramValue}\n";
            }
            return parameters;
        }
    }
}