using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateFilter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            //Create categories
            List<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(BuiltInCategory.OST_Doors));

            //Create elementfilter
            //ElementParameterFilter elementFilter = new ElementParameterFilter(ParameterFilterRuleFactory.CreateContainsRule(new ElementId(BuiltInParameter.VIEW_NAME), "Section 1", false));
            ParameterValueProvider provider = new ParameterValueProvider(new ElementId(BuiltInParameter.SYMBOL_NAME_PARAM));
            FilterRule filterRule = new FilterStringRule(provider, new FilterStringEquals(), "Cửa 2 mét");
            ElementParameterFilter elementFilter = new ElementParameterFilter(filterRule);

            try
            {
                using (Transaction trans = new Transaction(doc, "Create Filter"))
                {
                    trans.Start();
                    ParameterFilterElement filterElement = ParameterFilterElement.Create(doc, "My Filter", categories, elementFilter);

                    if (!doc.ActiveView.GetFilters().Contains(filterElement.Id))
                    {
                        doc.ActiveView.AddFilter(filterElement.Id);
                    }

                    //doc.ActiveView.SetFilterVisibility(filterElement.Id, false);
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetProjectionLineColor(new Color(255, 0, 0));
                    ogs.SetProjectionLineWeight(5);
                    ogs.SetHalftone(true);
                    ogs.SetSurfaceTransparency(50);
                    doc.ActiveView.SetFilterOverrides(filterElement.Id, ogs);
                    trans.Commit();
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