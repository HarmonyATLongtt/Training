using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BaiTap_Revit.P4
{
    [Transaction(TransactionMode.Manual)]
    internal class CreateFilter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Create categories
            List<ElementId> cats = new List<ElementId> { new ElementId(BuiltInCategory.OST_Walls) };
            View activeView = doc.ActiveView;

            try
            {
                using (Transaction tx = new Transaction(doc, "Create and Apply Filter"))
                {
                    tx.Start();

                    // Kiểm tra xem bộ lọc đã tồn tại chưa
                    FilteredElementCollector filterCollector = new FilteredElementCollector(doc)
                        .OfClass(typeof(ParameterFilterElement));
                    ParameterFilterElement existingFilter = filterCollector
                        .FirstOrDefault(f => f.Name == "Filter wall") as ParameterFilterElement;

                    if (existingFilter != null)
                    {
                        TaskDialog.Show("Warning", "The filter 'Filter wall' already exists.");
                        return Result.Cancelled;
                    }

                    // Create rule: Độ dày >= 0.5
                    ElementId widthParamId = new ElementId(BuiltInParameter.WALL_ATTR_WIDTH_PARAM);
                    if (widthParamId == null)
                    {
                        message = "Could not find the parameter for wall thickness.";
                        return Result.Failed;
                    }
                    FilterRule rule1 = ParameterFilterRuleFactory.CreateGreaterOrEqualRule(widthParamId, 0.5, 0);

                    // Rule: Tường ngoài
                    ElementId exteriorParamId = new ElementId(BuiltInParameter.FUNCTION_PARAM);
                    if (exteriorParamId == null)
                    {
                        message = "Could not find the parameter for wall function.";
                        return Result.Failed;
                    }
                    FilterRule rule2 = ParameterFilterRuleFactory.CreateEqualsRule(exteriorParamId, (int)WallFunction.Exterior);

                    // Combine rules
                    ElementParameterFilter filter1 = new ElementParameterFilter(rule1);
                    ElementParameterFilter filter2 = new ElementParameterFilter(rule2);
                    LogicalAndFilter elemFilter = new LogicalAndFilter(filter1, filter2);

                    // Create parameter filter
                    ParameterFilterElement paramFilter = ParameterFilterElement.Create(doc, "Filter wall", cats);
                    paramFilter.SetElementFilter(elemFilter);

                    // Customize visual appearance
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetCutLineColor(new Color(255, 0, 0));
                    ogs.SetCutLineWeight(5); // Thickness
                    ogs.SetSurfaceTransparency(5); // Transparency
                    ogs.SetProjectionLineColor(new Color(0, 255, 0)); // Projection line color
                    ogs.SetProjectionLineWeight(3); // Projection line thickness
                    ogs.SetSurfaceForegroundPatternColor(new Color(0, 0, 255)); // Surface pattern color
                    ogs.SetSurfaceForegroundPatternId(new ElementId(-1)); // No pattern

                    // Apply filter to active view
                    activeView.AddFilter(paramFilter.Id);
                    activeView.SetFilterVisibility(paramFilter.Id, true);
                    activeView.SetFilterOverrides(paramFilter.Id, ogs);

                    tx.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}