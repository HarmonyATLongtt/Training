using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class MyElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Get Referenct Element
                Reference r = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                // Get Element
                ElementId elementId = r.ElementId;
                Element element = doc.GetElement(elementId);

                // Get Information Of Element
                ElementId elementIdtype = element.GetTypeId();
                ElementType elementType = doc.GetElement(elementIdtype) as ElementType;

                if (r != null)
                {
                    //TaskDialog.Show("Element Infomation", "Category: " + element.Category.Name + Environment.NewLine
                    //    + "Name of element: " + element.Name + Environment.NewLine
                    //    + "Name of family: " + elementType.FamilyName + Environment.NewLine
                    //    + "Name of family type: " + elementType.Name
                    //    );

                    Parameter para = element.LookupParameter("Head Height");
                    InternalDefinition def = para.Definition as InternalDefinition;

                    TaskDialog.Show("Para info", string.Format("Name of para {0}, BuildtinPara type {1}",

                       def.Name,
                       def.BuiltInParameter
                       ));
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