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
    class GetPara : IExternalCommand
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
                    Parameter para = element.LookupParameter("Head Height");

                    InternalDefinition def = para.Definition as InternalDefinition;

                    TaskDialog.Show("Para Info ", string.Format("Tên của tham số là: {0}, Loại đơn vị là: {1}, Kiểu buildInParameter: {2}",
                        def.Name, 
                        def.UnitType, 
                        def.BuiltInParameter));
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
