using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using View = Autodesk.Revit.DB.View;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string templateFileName = "C:\\ProgramData\\Autodesk\\RVT 2021\\Family Templates\\English\\Metric Generic Model.rft";
            Document document = commandData.Application.Application.NewFamilyDocument(templateFileName);

            if (document == null)
                throw new Exception("Fail to create a new family document");

            try
            {
                using (var trans = new Transaction(document))
                {
                    trans.Start("Create new family");

                    Plane plane = Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), XYZ.Zero);
                    SketchPlane sketchPlane = SketchPlane.Create(document, plane);

                    var view = new FilteredElementCollector(document)
                        .OfCategory(BuiltInCategory.OST_Views)
                        .FirstOrDefault(e => e != null
                        && !(e as View).IsTemplate
                        && (e as View).IsViewValidForTemplateCreation()
                        && (e as View).Name.Equals("Left")) as View;

                    CurveArray curveArray = new CurveArray();
                    curveArray.Append(Line.CreateBound(new XYZ(-10, 10, 0), new XYZ(-10, -10, 0)));
                    curveArray.Append(Line.CreateBound(new XYZ(-10, -10, 0), new XYZ(10, -10, 0)));
                    curveArray.Append(Line.CreateBound(new XYZ(10, -10, 0), new XYZ(10, 10, 0)));
                    curveArray.Append(Line.CreateBound(new XYZ(10, 10, 0), new XYZ(-10, 10, 0)));

                    CurveArrArray profile = new CurveArrArray();
                    profile.Append(curveArray);

                    var extru = document.FamilyCreate.NewExtrusion(true, profile, sketchPlane, 10);

                    Options options = new Options();
                    options.ComputeReferences = true;

                    GeometryElement geometryElement = extru.get_Geometry(options);

                    var solid = geometryElement.FirstOrDefault(e => e is Solid);
                    var faceArrays = (solid as Solid).Faces;

                    ReferenceArray referenceArray = new ReferenceArray();

                    XYZ startPoint = XYZ.Zero;
                    XYZ endPoint = new XYZ(0, 0, 10);

                    PlanarFace topFace = null;
                    PlanarFace bottomFace = null;

                    foreach (PlanarFace e in faceArrays)
                    {
                        var check = e.FaceNormal;
                        if (check.X == 0 && check.Y == 0 && check.Z == 1)
                            topFace = e;
                        else if (check.X == 0 && check.Y == 0 && check.Z == -1)
                            bottomFace = e;
                    }

                    if (topFace != null && bottomFace != null)
                    {
                        referenceArray.Append(topFace.Reference);
                        referenceArray.Append(bottomFace.Reference);
                    }

                    Dimension dim = document.FamilyCreate.NewLinearDimension(view, Line.CreateBound(startPoint, endPoint), referenceArray);

                    FamilyParameter param = document.FamilyManager.AddParameter("Height",
                        BuiltInParameterGroup.PG_CONSTRAINTS,
                        ParameterType.Length, true);

                    dim.FamilyLabel = param;

                    trans.Commit();
                }

                string savePath = null;
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Revit family | *.rfa";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    savePath = dialog.FileName;
                    if (File.Exists(savePath))
                        File.Delete(savePath);
                }

                document.SaveAs(savePath);
                TaskDialog.Show("Message", "Create family successful");
            }
            catch { }

            return Result.Succeeded;
        }
    }
}