using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreatNewMaterial : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                using (Transaction trans = new Transaction(doc, "Creat New Material"))
                {
                    trans.Start();

                    #region Xóa vật liệu

                    //var material = new FilteredElementCollector(doc)
                    //                    .OfClass(typeof(Material))
                    //                    .WhereElementIsNotElementType()
                    //                    .Cast<Material>().ToList()
                    //                    .Where(p => p.Name == "Custom Material")
                    //                    .FirstOrDefault();
                    //doc.Delete(material.Id);

                    #endregion Xóa vật liệu

                    #region Tạo vật liệu

                    ElementId materialId = Material.Create(doc, "Custom Material");
                    Material newMaterial = doc.GetElement(materialId) as Material;

                    // Đặt các thuộc tính cho vật liệu
                    newMaterial.Color = new Color(255, 0, 0); // Màu đỏ
                    newMaterial.Transparency = 50; // 50% trong suốt
                    newMaterial.Shininess = 10; // Độ bóng trung bình
                    newMaterial.Smoothness = 10; // Độ mịn trung bình
                    //newMaterial.Name = "Custom Material";
                    //Đặt các thuộc tính mẫu cắt và bề mặt(nếu có)
                    newMaterial.CutBackgroundPatternColor = new Color(128, 128, 128); // Màu xám
                    newMaterial.SurfaceBackgroundPatternColor = new Color(255, 255, 255); // Màu trắng

                    // Tạo tập hợp thuộc tính vật lý mới
                    StructuralAsset strucAsset = new StructuralAsset("Property Set", StructuralAssetClass.Concrete);
                    strucAsset.Behavior = StructuralBehavior.Isotropic; //vật liệu đẳng hướng (isotropic material)
                    strucAsset.Density = UnitUtils.ConvertToInternalUnits(232.0, UnitTypeId.KilogramsPerCubicMeter); // Đặt mật độ đơn vị: kg/m³
                    PropertySetElement pse = PropertySetElement.Create(doc, strucAsset); // Tạo PropertySetElement từ StructuralAsset
                    newMaterial.SetMaterialAspectByPropertySet(MaterialAspect.Structural, pse.Id); // Gán tập hợp thuộc tính cho vật liệu

                    #endregion Tạo vật liệu

                    trans.Commit();
                }
                TaskDialog.Show("Information", "Tạo vật liệu thành công");
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}