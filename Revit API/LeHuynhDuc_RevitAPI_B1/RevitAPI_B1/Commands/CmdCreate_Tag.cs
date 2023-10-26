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
    [Transaction(TransactionMode.Manual)]
    public class CmdCreate_Tag : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
            TagOrientation tagOrientation = TagOrientation.Horizontal;

            IList<BuiltInCategory> cats = new List<BuiltInCategory>();
            cats.Add(BuiltInCategory.OST_Windows);
            cats.Add(BuiltInCategory.OST_Doors);

            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(cats);
            IList<Element> list = new FilteredElementCollector(doc, doc.ActiveView.Id)
                                    .WherePasses(filter)
                                    .WhereElementIsNotElementType()
                                    .ToElements();

            using (Transaction trans = new Transaction(doc, "Create_Tag"))
            {
                trans.Start();
                foreach (Element e in list)
                {
                    Reference reference = new Reference(e);
                    LocationPoint locationPoint = e.Location as LocationPoint;
                    XYZ loc = locationPoint.Point;
                    IndependentTag independentTag = IndependentTag.Create(doc, doc.ActiveView.Id, reference, true, tagMode, tagOrientation, loc);
                }
                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
