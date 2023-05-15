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

    class GetParaWalls : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);

                ICollection<Element> walls = collector.OfClass(typeof(Wall)).ToElements();

                

                foreach (Wall wall in walls)
                {
                    Parameter parameter = wall.LookupParameter("Location Line");
                    InternalDefinition def = parameter.Definition as InternalDefinition;
                    if (parameter != null)
                    {
                        string parameterName = parameter.Definition.Name;
                        TaskDialog.Show("Para Name", string.Format("Tên của tham số là: {0} \n Loại đơn vị là: {1} \n Kiểu buildInParameter: {2}",
                            def.Name,
                            def.UnitType,
                            def.BuiltInParameter));
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
