using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ChangeGridDistance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            List<Grid> grids = new List<Grid>();

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> gridElements = collector.OfClass(typeof(Grid)).ToElements();

            foreach (Element element in gridElements)
            {
                if (element is Grid grid)
                {
                    grids.Add(grid);
                }
            }
            //var grids = collector.OfClass(typeof(Grid));
            Grid grid1 = grids[0];
            Grid grid2 = grids[1];

            Line line1 = grid1.Curve as Line;
            Line line2 = grid2.Curve as Line;

            if (line1 == null && line2 == null)
            {
                TaskDialog.Show("Error", "Grid curves are not straight lines.");
                return Result.Failed;
            }

            if (!line1.Direction.IsAlmostEqualTo(line2.Direction) && !line1.Direction.IsAlmostEqualTo(-line2.Direction))
            {
                {
                    TaskDialog.Show("Error", "The grids are not parallel.");
                    return Result.Failed;
                }
            }
            XYZ pointOnGrid1 = line1.GetEndPoint(0);
            XYZ pointOnGrid2 = line2.Project(pointOnGrid1).XYZPoint;
            double currentDistance = pointOnGrid1.DistanceTo(pointOnGrid2);

            TaskDialog.Show("Current Distance", "Current distance between grids: " + currentDistance);
            double moveDistance = 5 - currentDistance;
            XYZ moveVector = line1.Direction.CrossProduct(XYZ.BasisZ).Normalize().Multiply(moveDistance);

            try
            {
                using (Transaction trans = new Transaction(doc, "Change Grid Distance"))
                {
                    trans.Start();
                    ElementTransformUtils.MoveElement(doc, grid2.Id, moveVector);

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