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
    class SetPara : IExternalCommand
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

                if (r != null)
                {
                    Parameter para = element.get_Parameter(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);

                    using (Transaction trans = new Transaction(doc, "Set Para"))
                    {
                        trans.Start();

                        para.Set(6.5);

                        trans.Commit();
                    }
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
