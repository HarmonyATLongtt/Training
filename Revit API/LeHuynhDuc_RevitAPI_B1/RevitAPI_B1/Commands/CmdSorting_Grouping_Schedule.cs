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
    public class CmdSorting_Grouping_Schedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //loc view 
            ViewSchedule vs = new FilteredElementCollector(doc)
                                       .OfCategory(BuiltInCategory.OST_Schedules)
                                       .First(x => x.Name == "Schedule1") as ViewSchedule;

            AddgroupingSchedule(vs, BuiltInParameter.SCHEDULE_LEVEL_PARAM, true, ScheduleSortOrder.Descending);

            return Result.Succeeded;
        }
        public void AddgroupingSchedule(ViewSchedule schedule, BuiltInParameter paramEnum, bool withTotalsAndDecoration, ScheduleSortOrder order)
        {
            //find field 
            ScheduleField field = FindScheduleField(schedule, paramEnum);
            // Build sort/group field.
            ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId, order);
            if (withTotalsAndDecoration)
            {
                sortGroupField.ShowFooter = true;
                sortGroupField.ShowFooterTitle = true;
                sortGroupField.ShowFooterCount = true;
                sortGroupField.ShowHeader = true;
                sortGroupField.ShowBlankLine = true;
            }

            // Add the sort/group field
            ScheduleDefinition definition = schedule.Definition;

            using (Transaction t = new Transaction(schedule.Document, "Add sort/group field"))
            {
                t.Start();
                definition.AddSortGroupField(sortGroupField);
                t.Commit();
            }
        }
        public ScheduleField FindScheduleField(ViewSchedule schedule, BuiltInParameter paramEnum)
        {
            ScheduleDefinition definition = schedule.Definition;
            ScheduleField foundField = null;
            ElementId paramId = new ElementId(paramEnum);

            foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
            {
                foundField = definition.GetField(fieldId);
                if (foundField.ParameterId == paramId)
                {
                    return foundField;
                }
            }

            return null;
        }

    }
}
