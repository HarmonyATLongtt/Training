using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FirstCommand.View;

namespace FirstCommand
{
    [Transaction(TransactionMode.Manual)]
    public class ComboboxView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            ViewComboboxWindow viewWindow = new ViewComboboxWindow(doc);

            if (viewWindow.ShowDialog() == true)
            {
                Autodesk.Revit.DB.View selectedView = viewWindow.SelectedView;

                if (selectedView != null)
                {
                    using (Transaction trans = new Transaction(doc, "Create Filter For View"))
                    {
                        trans.Start();

                        List<ElementId> categories = new List<ElementId>();
                        categories.Add(new ElementId(BuiltInCategory.OST_Walls));

                        ParameterValueProvider provider = new ParameterValueProvider(new ElementId(BuiltInParameter.SYMBOL_NAME_PARAM));
                        FilterRule filterRule = new FilterStringRule(provider, new FilterStringEquals(), "Tường xây gạch 80 xây vữa");
                        ElementParameterFilter elementFilter = new ElementParameterFilter(filterRule);

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
                    TaskDialog.Show("Success", "Filter has been created on the selected view.");
                }
                else
                {
                    TaskDialog.Show("Error", "No view was selected.");
                    return Result.Failed;
                }

                return Result.Succeeded;
            }
            return Result.Failed;
        }
    }
}