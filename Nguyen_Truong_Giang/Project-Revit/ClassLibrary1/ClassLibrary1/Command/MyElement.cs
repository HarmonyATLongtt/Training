using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class MyElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            try
            {
                //get Reference Element
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //get Element
                ElementId elementId = r.ElementId;
                Element element = doc.GetElement(elementId);

                //get information Element
                ElementId elementIdType = element.GetTypeId();
                ElementType elementType = doc.GetElement(elementIdType) as ElementType;

                if (r != null)
                {
                    TaskDialog.Show("- Element Information","- Category: " + element.Category.Name + Environment.NewLine
                        + "- Name of Element: " + element.Name + Environment.NewLine
                        + "- Name of Family: " + elementType.FamilyName + Environment.NewLine
                        + "- Name of Family Type - Symbol: " + elementType.Name);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
