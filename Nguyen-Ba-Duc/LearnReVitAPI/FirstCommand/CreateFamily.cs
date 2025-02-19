using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class CreateFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Tạo một tài liệu Family mới
                string templatePath = @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English-Imperial\Generic Model.rft";
                if (!System.IO.File.Exists(templatePath))
                {
                    TaskDialog.Show("Error", "Không tìm thấy file template Family.");
                    return Result.Failed;
                }

                Document familyDoc = doc.Application.NewFamilyDocument(templatePath);
                if (familyDoc == null)
                {
                    TaskDialog.Show("Error", "Không thể tạo tài liệu Family.");
                    return Result.Failed;
                }

                using (Transaction trans = new Transaction(familyDoc, "Create Extrusion"))
                {
                    trans.Start();
                    // Tạo một hình hộp chữ nhật (Extrusion) trong Family
                    SketchPlane sketchPlane = SketchPlane.Create(familyDoc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));
                    CurveArray curveArray = new CurveArray();
                    curveArray.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)));
                    curveArray.Append(Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 10, 0)));
                    curveArray.Append(Line.CreateBound(new XYZ(10, 10, 0), new XYZ(0, 10, 0)));
                    curveArray.Append(Line.CreateBound(new XYZ(0, 10, 0), new XYZ(0, 0, 0)));

                    CurveArrArray curveArrArray = new CurveArrArray();
                    curveArrArray.Append(curveArray);

                    Extrusion extrusion = familyDoc.FamilyCreate.NewExtrusion(true, curveArrArray, sketchPlane, 5.0);

                    //FamilyManager familyManager = familyDoc.FamilyManager;

                    //FamilyParameter param = familyManager.AddParameter( "CustomWidth",BuiltInParameterGroup.PG_GEOMETRY, FamilyParameter., false);
                    //// Gán giá trị cho Parameter
                    //if (param != null)
                    //{
                    //    familyManager.Set(param, 10.0);
                    //}

                    trans.Commit();
                }

                // Lưu Family ra file và load vào dự án
                string familyPath = @"E:\FamilyTemplate\NewFamily.rfa";
                familyDoc.SaveAs(familyPath);
                familyDoc.Close();

                using (Transaction tr = new Transaction(doc, "Load Family"))
                {
                    tr.Start();
                    Family family;
                    bool isLoaded = doc.LoadFamily(familyPath, out family);
                    if (!isLoaded || family == null)
                    {
                        TaskDialog.Show("Error", "Không thể load Family vào dự án.");
                        return Result.Failed;
                    }
                    TaskDialog.Show("Success", "Family đã được tạo và load thành công.");

                    tr.Commit();
                }

                using (Transaction tr = new Transaction(doc, "Load Family"))
                {
                    tr.Start();
                    FamilySymbol symbol = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .WhereElementIsElementType()
                        .Cast<FamilySymbol>()
                        .First(x => x.Name == "New Family");

                    tr.Commit();
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