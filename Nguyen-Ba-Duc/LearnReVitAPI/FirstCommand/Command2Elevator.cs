using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Command2Elevator : IExternalCommand
    {
        private double _elevator { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            AddElevationView addElevationWindow = new AddElevationView(doc);

            if (addElevationWindow.ShowDialog() == true)
            {
                Level selectedLevel = addElevationWindow.Level;

                if (selectedLevel != null)
                {
                    if (double.TryParse((addElevationWindow.Elevator), out double elevator))
                    {
                        _elevator = elevator;
                    }

                    if (_elevator != default)
                    {
                        List<Element> listInstances = new FilteredElementCollector(doc)
                                .OfClass(typeof(FamilyInstance))
                                .ToList();

                        if (listInstances.Count == 0)
                        {
                            TaskDialog.Show("Lỗi", "Không tìm thấy các Family Instance");
                            return Result.Failed;
                        }

                        try
                        {
                            using (Transaction trans = new Transaction(doc, "Add Elevator"))
                            {
                                trans.Start();

                                foreach (Element instance in listInstances)
                                {
                                    Parameter elevatorParam = instance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                                    Parameter levelParam = instance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                                    if (elevatorParam != null && levelParam != null)
                                    {
                                        //double currentOffset = elevatorParam.AsDouble();
                                        elevatorParam.Set(_elevator / 304.8);
                                        levelParam.Set(selectedLevel.Id);
                                    }
                                }

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
                    return Result.Failed;
                }
                return Result.Failed;
            }
            return Result.Failed;
        }
    }
}