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
    public class CmdFilter_Door : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // create list of categories that will for the filter
            List<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(BuiltInCategory.OST_Doors));

            // create a list of rules for the filter
            IList<FilterRule> filterRules = new List<FilterRule>();
            // This filter will have a single rule for the filter


            //ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //IList doors = collector.WherePasses(filter).WhereElementIsNotElementType().ToElements().ToList();
            //ICollection<ElementId> collectionId = new List<ElementId>();

            View currentView = doc.ActiveView;
            ParameterFilterElement parameterFilterElemen = null;
            using (Transaction trans = new Transaction(doc, "fillter_Doors"))
            {
                trans.Start();
                try
                {
                    parameterFilterElemen = ParameterFilterElement.Create(doc, "Filter Door", categories);
                    currentView.AddFilter(parameterFilterElemen.Id);

                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Message: ", ex.Message);
                }

                //foreach (Element e in doors)
                //{


                //    collectionId.Add(e.Id);
                //    currentView.SetElementOverrides(e.Id, overr);
                //}
                trans.Commit();
            }

            OverrideGraphicSettings overr = new OverrideGraphicSettings();
            overr.SetProjectionLineColor(new Color(255, 255, 0));
            using (Transaction trans = new Transaction(doc, "OverrideGraphic"))
            {
                trans.Start();
                currentView.SetFilterOverrides(parameterFilterElemen.Id, overr);
                trans.Commit();
            }
            //uidoc.Selection.SetElementIds(collectionId);
            return Result.Succeeded;
        }
    }
}
