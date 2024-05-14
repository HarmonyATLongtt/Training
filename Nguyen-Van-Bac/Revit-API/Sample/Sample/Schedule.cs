using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class Schedule : IExternalCommand
    {
        private BuiltInParameter[] BiParams = new BuiltInParameter[] { BuiltInParameter.ELEM_FAMILY_PARAM, BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.HOST_AREA_COMPUTED, BuiltInParameter.CURVE_ELEM_LENGTH };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)

        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction t = new Transaction(doc, "Create Schedule"))
            {
                t.Start();

                ElementId wallCategoryId = new ElementId(BuiltInCategory.OST_Walls);
                ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, wallCategoryId);
                schedule.Name = "Wall Schedule Sample";

                ScheduleSortGroupField familyTypeSorting = null;
                ScheduleFilter baseConstraintFilter = null;

                foreach (SchedulableField sf in schedule.Definition.GetSchedulableFields())
                {
                    if (CheckField(sf))
                    {
                        ScheduleField scheduleField = schedule.Definition.AddField(sf);
                        if (sf.ParameterId == new ElementId(BuiltInParameter.WALL_BASE_CONSTRAINT))
                        {
                            // Create ScheduleFilter by level
                            baseConstraintFilter = new ScheduleFilter(scheduleField.FieldId, ScheduleFilterType.Equal, GetLevelByName(doc, "Level 1").Id);
                            // Add schedule's filter
                            schedule.Definition.AddFilter(baseConstraintFilter);
                        }
                        // Schedule group sorting (to collect family and type field)
                        if (sf.ParameterId == new ElementId(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM))
                        {
                            familyTypeSorting = new ScheduleSortGroupField(scheduleField.FieldId);
                            schedule.Definition.AddSortGroupField(familyTypeSorting);
                        }
                    }
                }

                // Set filter and sorting/grouping only if they are not null
                if (baseConstraintFilter != null)
                {
                    schedule.Definition.SetFilter(0, baseConstraintFilter);
                }

                if (familyTypeSorting != null)
                {
                    schedule.Definition.SetSortGroupField(0, familyTypeSorting);
                }

                // Commit transaction
                t.Commit();
                uidoc.ActiveView = schedule;
            }

            // Set active view

            return Result.Succeeded;
        }

        public bool CheckField(SchedulableField vs)
        {
            foreach (BuiltInParameter bip in BiParams)
            {
                if (new ElementId(bip) == vs.ParameterId)
                    return true;
            }
            return false;
        }

        public Element GetLevelByName(Document doc, string name)
        {
            Level level = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().Where(x => x.Name == name).FirstOrDefault();
            return level;
        }
    }
}