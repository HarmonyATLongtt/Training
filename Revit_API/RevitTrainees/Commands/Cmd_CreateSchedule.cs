using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateSchedule : IExternalCommand
    {
        [Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            _ = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            ViewSchedule viewSchedule = null;
            try
            {
                using (var trans = new Transaction(doc, "Create new Schedule"))
                {
                    trans.Start();
                    // Create view schedule for walls
                    viewSchedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Walls));
                    // Regenerate document to accep and see changes
                    doc.Regenerate();
                    // Add three fields to current view schedule
                    AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS));
                    AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.HOST_AREA_COMPUTED));
                    AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.CURVE_ELEM_LENGTH));
                    AddRegularFieldToSchedule(viewSchedule, new ElementId(BuiltInParameter.WALL_USER_HEIGHT_PARAM));

                    // Add all fielablels in current view schedule
                    //AddFieldToSchedule(viewSchedule);

                    // Set style and format for schedule

                    trans.Commit();
                }

                ApplyFormattingToField(viewSchedule, 1);
                AddGroupingToSchedule(viewSchedule, BuiltInParameter.CURVE_ELEM_LENGTH, true, ScheduleSortOrder.Ascending);
            }
            catch { }

            return Result.Succeeded;
        }

        // Add a field to view schedule
        public void AddRegularFieldToSchedule(ViewSchedule viewSchedule, ElementId paramId)
        {
            ScheduleDefinition definition = viewSchedule.Definition;
            SchedulableField schedulableField = definition.GetSchedulableFields().FirstOrDefault(sf => sf.ParameterId == paramId);

            if (schedulableField != null)
            {
                definition.AddField(schedulableField);
            }
        }

        // Add all fieldables to view schedule
        public void AddFieldToSchedule(ViewSchedule viewSchedule)
        {
            // Get all field can add to this view schedule
            IList<SchedulableField> schedulableFields = viewSchedule.Definition.GetSchedulableFields();

            foreach (var sf in schedulableFields)
            {
                bool scheduleAlreadyAdded = false;

                // Get all id have in current view schedule in order
                IList<ScheduleFieldId> ids = viewSchedule.Definition.GetFieldOrder();
                foreach (ScheduleFieldId id in ids)
                {
                    if (viewSchedule.Definition.GetField(id).GetSchedulableField() == sf)
                    {
                        scheduleAlreadyAdded = true;
                        break;
                    }
                }

                if (!scheduleAlreadyAdded)
                    viewSchedule.Definition.AddField(sf);
            }
        }

        [System.Obsolete]
        public static void ApplyFormattingToField(ViewSchedule schedule, int fieldIndex)
        {
            // Get the field
            ScheduleField scheduleField = schedule.Definition.GetField(fieldIndex);

            // Build unit formatting for the field
            FormatOptions formatOptions = scheduleField.GetFormatOptions();
            formatOptions.UseDefault = false;
            formatOptions.DisplayUnits = DisplayUnitType.DUT_SQUARE_METERS;
            formatOptions.UnitSymbol = UnitSymbolType.UST_M_SUP_2;

            // Build style overrides for the field
            // Use override options to indicate fields that are overriden and apply changes
            TableCellStyle style = scheduleField.GetStyle();
            TableCellStyleOverrideOptions styleOptions = style.GetCellStyleOverrideOptions();

            styleOptions.BackgroundColor = true;
            style.BackgroundColor = new Color(80, 184, 231);
            styleOptions.FontColor = true;
            style.TextColor = new Color(255, 255, 255);
            styleOptions.Italics = true;
            style.IsFontItalic = true;

            style.SetCellStyleOverrideOptions(styleOptions);

            double width = scheduleField.GridColumnWidth;

            using (var trans = new Transaction(schedule.Document, "Set style etc"))
            {
                trans.Start();

                scheduleField.SetStyle(style);
                scheduleField.SetFormatOptions(formatOptions);
                scheduleField.GridColumnWidth = width + 0.2;

                trans.Commit();
            }
        }

        public static void AddGroupingToSchedule(ViewSchedule schedule, BuiltInParameter paramEnum, bool withTotalsAndDecoration, ScheduleSortOrder order)
        {
            // Get field
            var field = FindField(schedule, paramEnum);

            if (field == null)
            {
                throw new System.Exception("Unable to find field.");
            }

            // Built sort/ group field
            ScheduleSortGroupField sortGroupField = new ScheduleSortGroupField(field.FieldId, order);

            if (withTotalsAndDecoration)
            {
                sortGroupField.ShowFooter = true;
                sortGroupField.ShowFooterTitle = true;
                sortGroupField.ShowHeader = true;
                sortGroupField.ShowFooterCount = true;
            }

            using (var trans = new Transaction(schedule.Document, "Add sort/ group filter"))
            {
                trans.Start();

                schedule.Definition.AddSortGroupField(sortGroupField);

                trans.Commit();
            }
        }

        public static ScheduleField FindField(ViewSchedule schedule, BuiltInParameter paramEnum)
        {
            ScheduleDefinition definition = schedule.Definition;
            ScheduleField foundField = null;
            ElementId paramId = new ElementId(paramEnum);

            foreach (var fieldId in definition.GetFieldOrder())
            {
                foundField = definition.GetField(fieldId);
                if (foundField.ParameterId == paramId)
                    return foundField;
            }
            return null;
        }
    }
}