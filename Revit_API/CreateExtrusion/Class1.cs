using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace CreateExtrusion
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            XYZ origin = new XYZ(10, 0, 0);
            XYZ normal = new XYZ(0, 1, 0);

            using (var transaction = new Transaction(doc, "Create Extrusion"))
            {
                transaction.Start();

                CreateDirectShape(doc, CreateExtrusionGeometry(origin, normal));

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        private void CreateDirectShape(Document doc, Solid solid)
        {
            DirectShape dirShape = DirectShape.CreateElement(doc, Category.GetCategory(doc, BuiltInCategory.OST_GenericModel).Id);
            dirShape.SetShape(new List<GeometryObject>() { solid });
        }

        private Solid CreateExtrusionGeometry(XYZ origin, XYZ normal)
        {
            Plane currentPlane = Plane.CreateByNormalAndOrigin(normal, origin);

            double distance;
            UV uv = new UV();

            // Get distance form origin + XYZ(1, 1, 1) to reference of it on currentPlane
            currentPlane.Project(origin + new XYZ(2, 2, 2), out uv, out distance);

            // Calculate point is reference of origin + XYZ(1, 1, 1) on currentPlane
            // Because currentPlane.Normal return a normal was normalize, means currentPlane.Normal has dimension equal 1
            // After multiply normal with distance, we will get a point create with origin + XYZ(1, 1, 1) parallel lines
            // That means if we take origin + XYZ(1, 1, 1) - distance * normal, we will get reference in currentPlane
            XYZ first = origin + new XYZ(2, 2, 2) - currentPlane.Normal * distance;

            // Calculate third point use midpoint property
            // first + third = 2 * origin
            XYZ third = origin * 2 - new XYZ(first.X, first.Y, first.Z);

            // Get normal 2 perpendicular normal and vector direction of third and first
            XYZ v2 = normal.CrossProduct(third - first);

            // ========== Summary ========== //
            // We have normal and origin, we will create a straight lines by normal and origin
            // It has function (I will assign name 'd')
            // x = origin.x + normal.x * t
            // y = origin.y + normal.y * t
            // z = origin.z + normal.z * t
            // So if i have a point in 'd' (e), we will have distance from 'e' to origin by formula calculate distance between two point
            // And when use e - origin, we will get a vector direction the same v2 but have different dimension
            // e - origin = (normal.x * t, normal.y * t, normal.z * t)
            // And dimension equal Math.sqrt of sum of square x, y, z = t * dimension of normal
            // Means we have t * dimension of normal = distance(third and first)
            // So t = distance(third and first) / dimension(normal)
            // Have t, we will get second point
            // ============================= //

            // Calculate dimension of normal 2
            distance = XYZ.Zero.DistanceTo(new XYZ(v2.X, v2.Y, v2.Z));
            double t;
            if (distance != 0)
                t = origin.DistanceTo(first) / distance;
            else
                t = 0;

            // Calculate points the same first and third
            XYZ second = origin + t * new XYZ(v2.X, v2.Y, v2.Z);
            XYZ fourth = origin * 2 - new XYZ(second.X, second.Y, second.Z);

            // Create Curve loop
            CurveLoop squareLoop = new CurveLoop();
            squareLoop.Append(Line.CreateBound(first, second));
            squareLoop.Append(Line.CreateBound(second, third));
            squareLoop.Append(Line.CreateBound(third, fourth));
            squareLoop.Append(Line.CreateBound(fourth, first));

            IList<CurveLoop> profileLoops = new List<CurveLoop>();
            profileLoops.Add(squareLoop);

            // Create solid for shape
            return GeometryCreationUtilities.CreateExtrusionGeometry(profileLoops, normal, 20);
        }
    }
}
