using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BaiTap_Revit.P5
{// tag dùng gán nhãn và xác định các đối tượng
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Tag : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
            TagOrientation tagOrientation = TagOrientation.Horizontal;

            // Create Category
            List<BuiltInCategory> categories = new List<BuiltInCategory>();
            categories.Add(BuiltInCategory.OST_Windows);
            categories.Add(BuiltInCategory.OST_Doors);

            ElementMulticategoryFilter element = new ElementMulticategoryFilter(categories);

            IList<Element> list = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WherePasses(element)
                .WhereElementIsNotElementType().ToElements();

            try
            {
                using (Transaction Trans = new Transaction(doc, "Place Family"))
                {
                    Trans.Start();
                    foreach (Element element1 in list)
                    {
                        Reference reference = new Reference(element1);
                        LocationPoint location = element1.Location as LocationPoint;
                        XYZ point = location.Point;
                        IndependentTag tag = IndependentTag.Create(doc, doc.ActiveView.Id, reference, true, tagMode, tagOrientation, point);
                    }
                    Trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}