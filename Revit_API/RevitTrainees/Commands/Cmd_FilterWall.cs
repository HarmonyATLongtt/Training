using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Forms;
using System.Collections.Generic;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_FilterWall : IExternalCommand
    {
        [System.Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            View view = null;
            ParameterFilterElement filterElement = null;
            ElementId filterElementId = null;
            string filterName = "Filter wall area";

            // Create and show form
            Form_FilterWall f = new Form_FilterWall(doc);
            f.ShowDialog();
            view = f.SelectedView;

            // Get filter name if it was existed
            var checkFilterExist = new FilteredElementCollector(doc)
                .OfClass(typeof(FilterElement))
                .WhereElementIsNotElementType()
                .Cast<FilterElement>()
                .FirstOrDefault(e => e.Name.Equals(filterName));

            // Get a fill pattern
            var fillPattern = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .WhereElementIsNotElementType()
                .Cast<FillPatternElement>()
                .FirstOrDefault(e => e.Name.Equals("Aluminum"));

            // Get a line pattern
            var linePattern = new FilteredElementCollector(doc)
                .OfClass(typeof(LinePatternElement))
                .WhereElementIsNotElementType()
                .Cast<LinePatternElement>()
                .FirstOrDefault(e => e.Name.Equals("Hidden"));

            // Add object is target of filter
            List<ElementId> cats = new List<ElementId>
            {
                new ElementId(BuiltInCategory.OST_Walls)
            };

            // Get parameter to filter
            ElementId parameter = new ElementId(BuiltInParameter.HOST_AREA_COMPUTED);

            // Value of area to filter
            double value = UnitUtils.ConvertToInternalUnits(50.0f, DisplayUnitType.DUT_SQUARE_METERS);

            // Notify for user
            TaskDialog.Show("Filter", "Filter all walls have area greater than 50 square meters");

            // Create element filter
            ElementParameterFilter elementFilter
                = new ElementParameterFilter(ParameterFilterRuleFactory.CreateGreaterRule(parameter, value, 0.00001));

            // Create and set override graphics
            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
            overrideGraphicSettings.SetSurfaceForegroundPatternId(fillPattern.Id);
            overrideGraphicSettings.SetSurfaceForegroundPatternColor(new Color(0, 0, 0));
            overrideGraphicSettings.SetProjectionLinePatternId(linePattern.Id);
            overrideGraphicSettings.SetProjectionLineColor(new Color(255, 0, 0));
            overrideGraphicSettings.SetProjectionLineWeight(5);
            overrideGraphicSettings.SetCutLinePatternId(linePattern.Id);
            overrideGraphicSettings.SetCutLineColor(new Color(255, 0, 255));
            overrideGraphicSettings.SetCutBackgroundPatternId(fillPattern.Id);
            overrideGraphicSettings.SetCutForegroundPatternColor(new Color(255, 0, 0));
            overrideGraphicSettings.SetHalftone(false);

            try
            {
                if (checkFilterExist != null)
                {
                    using (var trans = new Transaction(doc, "Add filter in " + uiDoc.ActiveView.Name))
                    {
                        trans.Start();

                        // Chehck if filter was set, remove it
                        if (view.IsFilterApplied(checkFilterExist.Id))
                            view.RemoveFilter(checkFilterExist.Id);

                        // Add filter
                        view.AddFilter(checkFilterExist.Id);
                        view.SetFilterOverrides(checkFilterExist.Id, overrideGraphicSettings);

                        trans.Commit();
                    }
                    return Result.Succeeded;
                }

                using (var trans = new Transaction(doc, "Create new filter in " + uiDoc.ActiveView.Name))
                {
                    trans.Start();

                    // Create new filter when it haven't available
                    filterElement = ParameterFilterElement.Create(doc, filterName, cats, elementFilter);
                    filterElementId = new ElementId(filterElement.Id.IntegerValue);
                    view.AddFilter(filterElementId);
                    view.SetFilterOverrides(filterElementId, overrideGraphicSettings);

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}