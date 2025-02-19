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
    public class Combobox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            LevelWindow levelWindow = new LevelWindow(doc);
            if (levelWindow.ShowDialog() == true)
            {
                Level selectedLevel = levelWindow.SelectedLevel;

                if (selectedLevel != null)
                {
                    using (Transaction trans = new Transaction(doc, "Create Element"))
                    {
                        trans.Start();

                        FamilySymbol beamType = new FilteredElementCollector(doc)
                           .OfClass(typeof(FamilySymbol))
                           .OfCategory(BuiltInCategory.OST_StructuralFraming)
                           .Cast<FamilySymbol>()
                           .FirstOrDefault();

                        if (beamType == null)
                        {
                            TaskDialog.Show("Error", "Không tìm thấy loại dầm trong dự án.");
                            return Result.Failed;
                        }

                        if (!beamType.IsActive)
                        {
                            beamType.Activate();
                            doc.Regenerate();
                        }

                        // Định nghĩa đường cơ sở (Baseline) cho dầm
                        XYZ startPoint = new XYZ(0, 0, 0);
                        XYZ endPoint = new XYZ(10, 0, 0);
                        Line beamLine = Line.CreateBound(startPoint, endPoint);

                        FamilyInstance beam = doc.Create.NewFamilyInstance(beamLine, beamType, selectedLevel, StructuralType.Beam);

                        trans.Commit();
                    }
                    TaskDialog.Show("Success", "Element has been created on the selected level.");
                }
                else
                {
                    TaskDialog.Show("Error", "No level was selected.");
                    return Result.Failed;
                }

                return Result.Succeeded;
            }
            return Result.Failed;
        }
    }
}