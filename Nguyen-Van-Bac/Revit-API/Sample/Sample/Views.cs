using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateViewPlan : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction t = new Transaction(doc, "Create View Plan"))
            {
                t.Start();

                // Tìm Level để tạo View Plan
                Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .FirstOrDefault(e => e.Name.Equals("Level 1")) as Level;

                if (level == null)
                {
                    message = "Level 1 not found.";
                    return Result.Failed;
                }
                // Tạo View Plan
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.FloorPlan);

                if (viewFamilyType == null)
                {
                    message = "ViewFamilyType for Floor Plan not found.";
                    return Result.Failed;
                }

                ViewPlan viewPlan = ViewPlan.Create(doc, viewFamilyType.Id, level.Id);
                viewPlan.Name = "New Floor Plan";

                t.Commit();
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateViewSection : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            using (Transaction t = new Transaction(doc, "Create View Section"))
            {
                t.Start();
                Wall wall = new CreateWall().CreateWallUsingCurve1(doc, doc.ActiveView.GenLevel, uidoc, new XYZ(0, 0, 0), new XYZ(100, 0, 0));
                CreateSection(wall);
                t.Commit();
                TaskDialog.Show("Ok", "Tạo thành công");
            }
            return Result.Succeeded;
        }

        public void CreateSection(Wall wall)
        {
            Document document = wall.Document;
            //find a section view type
            IEnumerable<ViewFamilyType> viewFamilyTypes = from elem in new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType))
                                                          let type = elem as ViewFamilyType
                                                          where type.ViewFamily == ViewFamily.Section
                                                          select type;
            //Create a BoundingBoxXYZ instance centered on wall
            LocationCurve lc = wall.Location as LocationCurve;
            Transform curveTransform = lc.Curve.ComputeDerivatives(0.5, true);
            XYZ origin = curveTransform.Origin;
            XYZ viewDirection = curveTransform.BasisX.Normalize();
            XYZ norwal = viewDirection.CrossProduct(XYZ.BasisZ).Normalize();

            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = norwal;
            transform.BasisY = XYZ.BasisZ;
            transform.BasisZ = norwal.CrossProduct(XYZ.BasisZ);
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            sectionBox.Transform = transform;
            sectionBox.Min = new XYZ(-10, 0, 0);
            sectionBox.Max = new XYZ(10, 12, 5);
            ViewSection viewSection = ViewSection.CreateSection(document, viewFamilyTypes.First().Id, sectionBox);
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateView3D : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction t = new Transaction(doc, "Create View 3D"))
            {
                t.Start();

                // Tạo View 3D
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

                if (viewFamilyType == null)
                {
                    message = "ViewFamilyType for 3D View not found.";
                    return Result.Failed;
                }

                View3D view3D = View3D.CreateIsometric(doc, viewFamilyType.Id);
                view3D.Name = "New 3D View";

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}