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
    [TransactionAttribute(TransactionMode.ReadOnly)] // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class GetPara : IExternalCommand
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
                    Parameter para = element.LookupParameter("Head Height");
                    InternalDefinition def = para.Definition as InternalDefinition;
                    TaskDialog.Show("Parameter Infor", string.Format("Ten cua tham so la {0}, loại don vi cua la {1}, kieu BuiltInParameter {2}",
                        def.Name,
                        def.UnitType,
                        def.BuiltInParameter));
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