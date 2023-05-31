using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyCommand1
{
    [TransactionAttribute(TransactionMode.ReadOnly)]  // giúp revit hiểu cách đọc lệnh này như thế nào
    public class MyElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;
            try
            {
                // get reference element
                Reference refer = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                // get element

                ElementId elementId = refer.ElementId;
                Element element = doc.GetElement(elementId);

                // get information of element
                ElementId elementIdType = element.GetTypeId();
                ElementType elementType = doc.GetElement(elementIdType) as ElementType;

                if (refer != null)
                {
                    TaskDialog.Show("Element Information", "Category: " + element.Category.Name + Environment.NewLine
                                    + "Family: " + elementType.FamilyName + Environment.NewLine
                                    + "Type - Symbol: " + elementType.Name + Environment.NewLine
                                    + "Element : " + element.Name);
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}