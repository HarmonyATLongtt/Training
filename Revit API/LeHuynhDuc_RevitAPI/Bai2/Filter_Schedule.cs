using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    internal class Filter_Schedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            ViewSchedule viewSchedule = new FilteredElementCollector(doc)
                                            .OfClass(typeof(ViewSchedule))
                                            .Cast<ViewSchedule>()
                                            .First(x => x.Name == "Schedule1");
            
            Element level = new FilteredElementCollector(doc)
                                     .OfClass(typeof(Level))
                                     .Cast<Level>()
                                     .First(x => x.Name == "Level 2") as Element;
            
            AddFilteredToSchedule(viewSchedule, level.Id);
            return Result.Succeeded;
        }
        public void AddFilteredToSchedule(ViewSchedule schedule, ElementId levelId)
        {
            // Find level field
            ScheduleDefinition definition = schedule.Definition;
            ScheduleField levelField = FindField(schedule, BuiltInParameter.SCHEDULE_LEVEL_PARAM);

            TaskDialog.Show("Thong bao", levelField.GetName());
            using (Transaction trans = new Transaction(schedule.Document, "Add filtered"))
            {
                trans.Start();
                // If field not present, add it
                if (levelField == null)
                {
                     levelField = definition.AddField(ScheduleFieldType.Instance, new ElementId(BuiltInParameter.SCHEDULE_LEVEL_PARAM));
                }
                // Set field to hidden
                //levelField.IsHidden = true;
                ScheduleFilter filter = new ScheduleFilter(levelField.FieldId, ScheduleFilterType.Equal, levelId);
                definition.AddFilter(filter);
                trans.Commit();
            }
        }
        public ScheduleField FindField(ViewSchedule schedule, BuiltInParameter paramEnum)
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
