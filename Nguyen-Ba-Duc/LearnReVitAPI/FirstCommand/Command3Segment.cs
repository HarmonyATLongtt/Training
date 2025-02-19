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
    public class Command3Segment : IExternalCommand
    {
        public List<MaterialAndLevel> materialAndLevels { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Chọn Family Instance
            Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Chọn một Family Instance");

            if (pickedRef != null)
            {
                // Get Element
                ElementId elementId = pickedRef.ElementId;
                Element element = doc.GetElement(elementId);

                if (element == null)
                {
                    TaskDialog.Show("Error", "Vui lòng chọn một Family Instance.");
                    return Result.Failed;
                }

                InputMaterialView inputMaterialView = new InputMaterialView(doc);
                if (inputMaterialView.ShowDialog() == true)
                {
                    materialAndLevels = inputMaterialView.materialAndLevels.OrderBy(x => x.Level.Elevation).ToList();

                    if (element.GetType() != typeof(FamilyInstance))
                    {
                        TaskDialog.Show("Error", "Element is not a Family Instance.");
                        return Result.Failed;
                    }
                    FamilyInstance familyInstance = element as FamilyInstance;

                    using (Transaction trans = new Transaction(doc, "Split and Clone Family Instance"))
                    {
                        trans.Start();

                        //Lấy hình học của Family Instance
                        Solid originalSolid = GetSolidFromFamilyInstance(element);
                        if (originalSolid == null)
                        {
                            TaskDialog.Show("Error", "Không thể lấy hình học của Family Instance.");
                            return Result.Failed;
                        }

                        // Cắt Family theo 3 Level
                        Dictionary<Solid, Material> cutSolids = CutSolidByLevels(originalSolid, materialAndLevels);

                        // Clone Family với kích thước từ các khối đã cắt
                        CloneFamilyInstances(doc, familyInstance, cutSolids);

                        trans.Commit();
                    }

                    return Result.Succeeded;
                }
                return Result.Failed;
            }
            return Result.Failed;
        }

        private Solid GetSolidFromFamilyInstance(Element element)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElement = element.get_Geometry(options);
            foreach (GeometryObject geomObj in geomElement)
            {
                if (geomObj is Solid solid)
                {
                    if (solid.Faces.Size > 0 && solid.Volume > 0)
                    {
                        return solid;
                    }
                }
                if (geomObj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObj in instanceGeometry)
                    {
                        if (geometryObj is Solid nestedSolid)
                        {
                            if (nestedSolid.Faces.Size > 0 && nestedSolid.Volume > 0)
                            {
                                return nestedSolid;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Dictionary<Solid, Material> CutSolidByLevels(Solid solid, List<MaterialAndLevel> materialAndLevels)
        {
            Dictionary<Solid, Material> cutSolids = new Dictionary<Solid, Material>();

            Level top = materialAndLevels[2].Level;
            Level middle = materialAndLevels[1].Level;
            Level bottom = materialAndLevels[0].Level;

            Material materialTop = materialAndLevels[2].Material;
            Material materialMiddle = materialAndLevels[1].Material;
            Material materialBottom = materialAndLevels[0].Material;

            // Mặt phẳng cắt từ các Level
            Plane planeBottom = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, bottom.Elevation));
            Plane planeMiddle = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, middle.Elevation));
            Plane planeTop = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, top.Elevation));

            // Cắt từng phần riêng biệt theo đúng thứ tự
            Solid part1 = BooleanOperationsUtils.CutWithHalfSpace(solid, planeTop); // Phần trên LevelTop
            Solid remaining1 = BooleanOperationsUtils.CutWithHalfSpace(solid, Plane.CreateByNormalAndOrigin(XYZ.BasisZ.Negate(), new XYZ(0, 0, top.Elevation)));

            Solid part2 = BooleanOperationsUtils.CutWithHalfSpace(remaining1, planeMiddle); // Phần giữa LevelTop - LevelMiddle
            Solid remaining2 = BooleanOperationsUtils.CutWithHalfSpace(remaining1, Plane.CreateByNormalAndOrigin(XYZ.BasisZ.Negate(), new XYZ(0, 0, middle.Elevation)));

            Solid part3 = BooleanOperationsUtils.CutWithHalfSpace(remaining2, planeBottom); // Phần giữa LevelMiddle - LevelBottom
            Solid part4 = BooleanOperationsUtils.CutWithHalfSpace(remaining2, Plane.CreateByNormalAndOrigin(XYZ.BasisZ.Negate(), new XYZ(0, 0, bottom.Elevation))); // Phần dưới LevelBottom

            // Chỉ thêm Solid nếu nó tồn tại và có thể tích khác 0
            if (part4 != null && part4.Volume > 0) cutSolids.Add(part4, null);
            if (part3 != null && part3.Volume > 0) cutSolids.Add(part3, materialBottom);
            if (part2 != null && part2.Volume > 0) cutSolids.Add(part2, materialMiddle);
            if (part1 != null && part1.Volume > 0) cutSolids.Add(part1, materialTop);

            return cutSolids;
        }

        private void CloneFamilyInstances(Document doc, FamilyInstance originalInstance, Dictionary<Solid, Material> cutSolids)
        {
            FamilySymbol symbol = originalInstance.Symbol;
            XYZ originalLocation = (originalInstance.Location as LocationPoint).Point;

            // Vị trí dịch chuyển ngang (sang phải trên trục X)
            double offsetX = 15;
            double originY = originalLocation.Y;
            double offsetZ = originalLocation.Z;

            foreach (var solidDict in cutSolids)
            {
                // Tính toán kích thước mới của instance dựa trên Solid
                BoundingBoxXYZ bbox = solidDict.Key.GetBoundingBox();
                double width = bbox.Max.X - bbox.Min.X;
                double length = bbox.Max.Y - bbox.Min.Y;
                double height = bbox.Max.Z - bbox.Min.Z;

                Material material = solidDict.Value;

                XYZ newLocation = new XYZ(offsetX, originY, offsetZ);

                // Tạo Family Instance mới
                FamilyInstance newInstance = doc.Create.NewFamilyInstance(newLocation, symbol, StructuralType.NonStructural);

                // Áp dụng kích thước mới
                Parameter widthParam = newInstance.LookupParameter("Chieu Rong");
                Parameter lengthParam = newInstance.LookupParameter("Chieu Dai");
                Parameter heightParam = newInstance.LookupParameter("Chieu Cao");
                Parameter materialParam = newInstance.LookupParameter("Nguyen Lieu");

                if (widthParam != null) widthParam.Set(width);
                if (lengthParam != null) lengthParam.Set(length);
                if (heightParam != null) heightParam.Set(height);
                if (materialParam != null && material != null) materialParam.Set(material.Id);

                offsetZ += height;
            }
        }
    }

    public class MaterialAndLevel
    {
        public Material Material { get; set; }

        public Level Level { get; set; }

        public MaterialAndLevel(Level level, Material material)
        {
            this.Material = material;
            this.Level = level;
        }
    }
}