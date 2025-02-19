using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            List<Curve> curveList = new List<Curve>
                {
                    Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)),
                    Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 10, 0)),
                    Line.CreateBound(new XYZ(10, 10, 0), new XYZ(0, 10, 0)),
                    Line.CreateBound(new XYZ(0, 10, 0), new XYZ(0, 0, 0))
                };

            CurveLoop curveLoop = CurveLoop.Create(curveList);

            FloorType floorType = new FilteredElementCollector(doc)
                    .OfClass(typeof(FloorType))
                    .Cast<FloorType>()
                    .FirstOrDefault();

            if (floorType == null)
            {
                throw new System.Exception("Floor type not found.");
            }
            Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault();

            if (level == null)
            {
                throw new System.Exception("No level found.");
            }

            try
            {
                using (Transaction trans = new Transaction(doc, "Create Floor"))
                {
                    trans.Start();

                    Floor floor = Floor.Create(doc, new List<CurveLoop> { curveLoop }, floorType.Id, level.Id);

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