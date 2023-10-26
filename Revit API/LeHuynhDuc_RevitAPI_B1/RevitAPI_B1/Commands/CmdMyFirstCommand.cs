using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class CmdMyFirstCommand : IExternalCommand
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
                        + "Name of Element:" + element.Name + Environment.NewLine
                        + "Name of family:" + elementType.FamilyName + Environment.NewLine
                        + "Name of familyType - Sympol" + elementType.Name);
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
