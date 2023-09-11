using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Filter
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Main : IExternalCommand
    {
        [System.Obsolete]
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Declare filter name
            string filterName = "My Filter";

            // Check filter name is exists ?
            var checkFilterNameExists = new FilteredElementCollector(doc)
                .OfClass(typeof(FilterElement))
                .WhereElementIsNotElementType()
                .Cast<FilterElement>()
                .FirstOrDefault(e => e.Name.Equals((filterName)));

            // If filter name is exists -> add to current view
            if (checkFilterNameExists != null)
            {
                using (var trans = new Transaction(doc, "Add filter to current view"))
                {
                    trans.Start();

                    doc.ActiveView.AddFilter(checkFilterNameExists.Id);

                    trans.Commit();
                }
                return Result.Succeeded;
            }

            // Get line pattern type 'hidden'
            var linePatternId = new FilteredElementCollector(doc)
                .OfClass(typeof(LinePatternElement))
                .WhereElementIsNotElementType()
                .Where((e => e.Name.Equals("Hidden")))
                .First().Id;

            // Get fill pattern type Aluminum
            var fillPatternId = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .WhereElementIsNotElementType()
                .Where(e => e.Name.Equals("Aluminum"))
                .First().Id;

            // Create a list type element id to save categories to filter
            List<ElementId> cats = new List<ElementId>();
            cats.Add(new ElementId(BuiltInCategory.OST_Floors));

            // Get parameter to filter on category
            ElementId parameter = new ElementId(BuiltInParameter.HOST_AREA_COMPUTED);

            // Create override graphic
            OverrideGraphicSettings overrideGraphic = new OverrideGraphicSettings();
            overrideGraphic.SetSurfaceForegroundPatternColor((new Color(255, 0, 0)));
            overrideGraphic.SetSurfaceForegroundPatternId(fillPatternId);
            overrideGraphic.SetProjectionLinePatternId(linePatternId);
            overrideGraphic.SetProjectionLineColor(new Color(125, 25, 200));

            using (var trans = new Transaction(doc, "Create my filter..."))
            {
                trans.Start();

                // Create element parameter filter to filter element
                ElementParameterFilter elementFilter;

                // Create a form enter input from user
                Form1 form = new Form1();
                form.ShowDialog();

                // Convert units user to units internal project
                float area = (float)UnitUtils.ConvertToInternalUnits(form.area, DisplayUnitType.DUT_SQUARE_METERS);

                switch (form.compareArea)
                {
                    case "Equals":
                        elementFilter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateEqualsRule(parameter, area, 0.001));
                        break;
                    case "Greater than":
                        elementFilter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateGreaterRule(parameter, area, 0.001));
                        break;
                    case "Less than":
                        elementFilter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateLessRule(parameter, area, 0.001));
                        break;
                    default:
                        elementFilter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateGreaterRule(parameter, 0, 0.001));
                        break;
                }

                // Create parameter filter element
                ParameterFilterElement filterElement = ParameterFilterElement.Create(doc, filterName, cats, elementFilter);
                // Add filter to current view
                doc.ActiveView.AddFilter(filterElement.Id);

                // Set filter override
                doc.ActiveView.SetFilterOverrides(filterElement.Id, overrideGraphic);

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
