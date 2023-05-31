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
    [TransactionAttribute(TransactionMode.Manual)] // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class SetPara : IExternalCommand
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

                if (refer != null)
                {
                    Parameter para = element.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);
                    using (Transaction trans = new Transaction(doc, "set para"))
                    {
                        trans.Start();

                        para.Set(6.5);

                        trans.Commit();
                    }
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