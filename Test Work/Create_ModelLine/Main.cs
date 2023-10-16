using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Create_ModelLine.Forms;
using System.Collections.Generic;

namespace Create_ModelLine
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            List<XYZ> points = new List<XYZ>();
            CurveArray curveArray = new CurveArray();
            ModelLineForm f = null;

            try
            {
                using (var trans = new Transaction(doc))
                {
                    trans.Start("Pick point");
                    f = new ModelLineForm(doc);
                    f.ShowDialog();

                    if (f.Break)
                        return Result.Succeeded;

                    trans.Commit();

                    while (true)
                    {
                        points.Add(uiDoc.Selection.PickPoint("Pick point to create lines..."));
                    }
                }
            }
            catch
            {
                try
                {
                    using (var trans = new Transaction(doc, "Create Model Lines..."))
                    {
                        trans.Start();

                        XYZ origin;
                        if (f.Level != null)
                        {
                            origin = new XYZ(0, 0, f.Level.Elevation);
                        }
                        else
                            origin = new XYZ(0, 0, f.SelectedView.GenLevel.Elevation);

                        for (int i = 0; i < points.Count; i++)
                            points[i] = new XYZ(points[i].X, points[i].Y, origin.Z);

                        for (int i = 0; i < points.Count - 1; i++)
                            curveArray.Append(Line.CreateBound(points[i], points[i + 1]));
                        curveArray.Append(Line.CreateBound(points[points.Count - 1], points[0]));

                        if (f.detailCheck)
                        {
                            DetailCurveArray detailCurveArray = doc.Create.NewDetailCurveArray(f.SelectedView, curveArray) as DetailCurveArray;
                            foreach (DetailLine detailLine in detailCurveArray)
                                detailLine.LineStyle = doc.GetElement(f.ModelLineSelect);
                        }
                        else
                        {
                            SketchPlane plane = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(new XYZ(0, 0, 1), origin));
                            ModelCurveArray modelCurveArray = doc.Create.NewModelCurveArray(curveArray, plane) as ModelCurveArray;
                            foreach (ModelLine modelLine in modelCurveArray)
                                modelLine.LineStyle = doc.GetElement(f.ModelLineSelect);
                        }

                        trans.Commit();
                    }
                }
                catch { }
            }

            return Result.Succeeded;
        }
    }
}