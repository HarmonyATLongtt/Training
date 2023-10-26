using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Material = Autodesk.Revit.DB.Material;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using View = Autodesk.Revit.DB.View;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    public class Create_NewFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Application app = uiApp.Application;
            Document doc = uidoc.Document;

            Create(app);
            return Result.Succeeded;
        }
        public void Create(Application app)
        {
            string templatePath = @"C:\ProgramData\Autodesk\RVT 2021\Family Templates\English\Metric Furniture.rft";
            Document familyDoc = app.NewFamilyDocument(templatePath);
            FamilyManager familyManager = familyDoc.FamilyManager;

            XYZ pA = new XYZ(0, 3, 0);
            XYZ pB = new XYZ(2, 4, 0);
            XYZ pC = new XYZ(3, 3, 0);
            XYZ pD = new XYZ(2, 0.83, 0);
            XYZ pE = new XYZ(0, -0.45, 0);
            XYZ pF = new XYZ(-3, 3, 0);
            XYZ pG = new XYZ(-2, 4, 0);
            XYZ pH = new XYZ(-2, 0.83, 0);
            XYZ pI = new XYZ(-2.94, 1.7, 0);
            XYZ pJ = new XYZ(2.94, 1.7, 0);
            CurveArray curveArray = new CurveArray();
            curveArray.Append(Arc.Create(pA, pC, pB));
            //curveArray.Append(Line.CreateBound(pC, pD));
            curveArray.Append(Arc.Create(pC, pD, pJ));
            curveArray.Append(Line.CreateBound(pD, pE));
            curveArray.Append(Line.CreateBound(pE, pH));
            //curveArray.Append(Line.CreateBound(pH, pF));
            curveArray.Append(Arc.Create(pH, pF, pI));
            curveArray.Append(Arc.Create(pF, pA, pG));
            CurveArrArray curveArrArray = new CurveArrArray();
            curveArrArray.Append(curveArray);
            using (Transaction trans = new Transaction(familyDoc, "create family"))
            {
                trans.Start();
                // Tạo một loại mới
                FamilyType familyType = familyManager.NewType("1 x 1");
                XYZ origin = XYZ.Zero; // Vị trí gốc của hình
                XYZ xAxis = XYZ.BasisX; // Hướng trục X của hình
                XYZ yAxis = XYZ.BasisY; // Hướng trục Y của hình
                Plane plane = Plane.CreateByOriginAndBasis(origin, xAxis, yAxis);
                SketchPlane sketchPlane = SketchPlane.Create(familyDoc, plane);
                //View activeView = familyDoc.ActiveView;
                //SketchPlane sketchPlane = SketchPlane.Create(familyDoc, Plane.CreateByNormalAndOrigin(activeView.ViewDirection, XYZ.Zero));
                //foreach (Curve curve in curveArray)
                //{
                //   // ModelCurve modelCurve = familyDoc.FamilyCreate.NewModelCurve(curve, sketchPlane);
                //}

                Extrusion extrusion = familyDoc.FamilyCreate.NewExtrusion(true, curveArrArray, sketchPlane, 1);

                FamilyParameter materialParameter = familyManager.AddParameter(
                "Material1", // Tên tham số vật liệu
                BuiltInParameterGroup.PG_MATERIALS, // Nhóm tham số vật liệu
                ParameterType.Material, // Kiểu dữ liệu tham số vật liệu
                true // Không cho phép giá trị trống
                );
                //TaskDialog.Show("mess", materialParameter.Id.ToString());

                Parameter para = extrusion.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                para.Set(materialParameter.Id);
                familyDoc.FamilyManager.AssociateElementParameterToFamilyParameter(para, materialParameter);

                trans.Commit();
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // Thiết lập các thuộc tính cho hộp thoại lưu tệp tin
            saveFileDialog.Title = "Chọn đường dẫn lưu tệp tin";
            saveFileDialog.Filter = "Revit Family Files (*.rfa)|*.rfa|All Files (*.*)|*.*";
            saveFileDialog.InitialDirectory = @"D:\ThucTap\Revit\Family";
            var result = saveFileDialog.ShowDialog();

            // Hiển thị hộp thoại lưu tệp tin và kiểm tra kết quả
            if (result == true)
            {
                string filePath = saveFileDialog.FileName;
                familyDoc.SaveAs(filePath);
                familyDoc.Close(false);
                TaskDialog.Show("Thông báo", "Tệp tin đã được lưu tại: " + filePath);
            }
        }
    }
}
