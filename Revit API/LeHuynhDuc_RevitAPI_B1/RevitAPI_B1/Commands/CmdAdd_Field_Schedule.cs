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
    internal class CmdAdd_Field_Schedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> scheduleElements = collector.OfClass(typeof(ViewSchedule)).ToElements();

            // Lọc chỉ các ViewSchedule
            List<ViewSchedule> schedules = new List<ViewSchedule>();
            foreach (Element element in scheduleElements)
            {
                ViewSchedule viewSchedule = element as ViewSchedule;
                if (viewSchedule != null)
                {
                    schedules.Add(viewSchedule);
                }
            }
            using (Transaction trans = new Transaction(doc, "Add field to Schedule"))
            {
                trans.Start();
                AddFieldToSchedule(schedules);
                trans.Commit();
            }
            return Result.Succeeded;
        }
        /// <summary>
        /// Add fields to view schedule.
        /// </summary>
        public void AddFieldToSchedule(List<ViewSchedule> schedules)
        {
            IList<SchedulableField> schedulableFields = null;

            foreach (ViewSchedule vs in schedules)
            {
                //Get all schedulable fields from view schedule definition.
                schedulableFields = vs.Definition.GetSchedulableFields();

                foreach (SchedulableField sf in schedulableFields)
                {
                    bool fieldAlreadyAdded = false;
                    //Get all schedule field ids
                    IList<ScheduleFieldId> ids = vs.Definition.GetFieldOrder();
                    foreach (ScheduleFieldId id in ids)
                    {
                        //If the GetSchedulableField() method of gotten schedule field returns same schedulable field,
                        // it means the field is already added to the view schedule.
                        if (vs.Definition.GetField(id).GetSchedulableField() == sf)
                        {
                            fieldAlreadyAdded = true;
                            break;
                        }
                    }

                    //If schedulable field doesn't exist in view schedule, add it.
                    if (fieldAlreadyAdded == false)
                    {
                        vs.Definition.AddField(sf);
                    }
                }
            }
        }
    }
}
