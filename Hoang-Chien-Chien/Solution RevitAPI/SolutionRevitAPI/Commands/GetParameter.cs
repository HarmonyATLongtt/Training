using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class GetParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                if (reference != null)
                {
                    string resultPara = string.Empty;
                    Element element = doc.GetElement(reference);
                    foreach (Parameter para in element.Parameters)
                    {
                        if (string.IsNullOrEmpty(para.AsValueString()))
                            continue;

                        resultPara += $"Parameters: {para.Definition.Name}: {para.AsValueString()} \n";
                    }
                    TaskDialog.Show($"Thông tin parameter của đối tượng: {element.Name}", resultPara);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}