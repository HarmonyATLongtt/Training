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
    public class CreateBeam : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Get level
            Level level = new FilteredElementCollector(doc)
                   .OfClass(typeof(Level))
                   .Cast<Level>()
                   .FirstOrDefault();
            if (level == null)
            {
                TaskDialog.Show("Error", "Không tìm thấy Level trong dự án.");
                return Result.Failed;
            }

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

            try
            {
                using (Transaction trans = new Transaction(doc, "Create beam"))
                {
                    trans.Start();

                    FamilyInstance beam = doc.Create.NewFamilyInstance(beamLine, beamType, level, StructuralType.Beam);

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