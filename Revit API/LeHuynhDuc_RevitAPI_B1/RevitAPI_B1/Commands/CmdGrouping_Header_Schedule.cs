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
    internal class CmdGrouping_Header_Schedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            CreateSingleCategoryScheduleWithGroupedColumnHeaders(doc);
            return Result.Succeeded;
        }
        public void CreateSingleCategoryScheduleWithGroupedColumnHeaders(Document doc)
        {
            using (Transaction t = new Transaction(doc, "Create single-category with grouped column headers"))
            {
                // Build the schedule
                t.Start();
                ViewSchedule vs = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Doors));

                AddRegularField(vs, new ElementId(BuiltInParameter.WINDOW_HEIGHT));
                AddRegularField(vs, new ElementId(BuiltInParameter.WINDOW_WIDTH));
                AddRegularField(vs, new ElementId(BuiltInParameter.ALL_MODEL_MARK));
                AddRegularField(vs, new ElementId(BuiltInParameter.ALL_MODEL_COST));

                doc.Regenerate();

                // Group the headers in the body section using ViewSchedule methods
                vs.GroupHeaders(0, 0, 0, 1, "Size");
                vs.GroupHeaders(0, 2, 0, 3, "Other");
                vs.GroupHeaders(0, 0, 0, 3, "All");

                t.Commit();
            }

        }

        public void AddRegularField(ViewSchedule schedule, ElementId paramId)
        {
            ScheduleDefinition definition = schedule.Definition;

            // Find a matching SchedulableField
            SchedulableField schedulableField =
                definition.GetSchedulableFields().FirstOrDefault<SchedulableField>(sf => sf.ParameterId == paramId);

            if (schedulableField != null)
            {
                // Add the found field
                definition.AddField(schedulableField);
            }
        }
    }
}
