using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    public class MyFirstCommand : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uIDoc = commandData.Application.ActiveUIDocument;
            Document doc = uIDoc.Document;
           
            try
            {
                Reference r = uIDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                ElementId elementId = r.ElementId;
                Element element = doc.GetElement(elementId);
                ElementId elementIdType = element.GetTypeId();
                ElementType elementType = doc.GetElement(elementIdType) as ElementType;


                if (r != null)
                {
                    TaskDialog.Show("Element Infomation", "Category:" + element.Category.Name + Environment.NewLine
                        + "Name of Element:" + element.Name  + Environment.NewLine
                        + "Name of family:" + elementType.FamilyName + Environment.NewLine
                        + "Name of familyType - Sympol" + elementType.Name);
                }
                return Result.Succeeded;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
           
        }
    }
}
