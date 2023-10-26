using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class Create_Floor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //XYZ pA = new XYZ(6, -4, 0);
            //XYZ pB = new XYZ(6, 4, 0);
            //XYZ pC = new XYZ(10, 0, 0);
            //XYZ pD = new XYZ(2, 4, 0);
            //XYZ pE = new XYZ(2, 6, 0);
            //XYZ pF = new XYZ(0, 8, 0);
            //XYZ pG = new XYZ(-2, 6, 0);
            //XYZ pH = new XYZ(-2, 4, 0);
            //XYZ pI = new XYZ(-10, 4, 0);
            //XYZ pJ = new XYZ(-10, 6, 0);
            //XYZ pK = new XYZ(-12, 8, 0);
            //XYZ pL = new XYZ(-12, 4, 0);
            //XYZ pM = new XYZ(-10, -4, 0);
            //XYZ pN = new XYZ(-10, -6, 0);
            //XYZ pO = new XYZ(-12, -8, 0);
            //XYZ pP = new XYZ(-12, -4, 0);
            //XYZ pQ = new XYZ(-2, -4, 0);
            //XYZ pR = new XYZ(-2, -6, 0);
            //XYZ pS = new XYZ(2, -4, 0);
            //XYZ pT = new XYZ(2, -6, 0);
            //XYZ pU = new XYZ(0, -8, 0);

            //CurveArray curveArray = new CurveArray();
            //curveArray.Append(Arc.Create(pA, pB, pC));
            //curveArray.Append(Line.CreateBound(pB, pD));
            //curveArray.Append(Line.CreateBound(pD, pE));
            //curveArray.Append(Arc.Create(pE, pG, pF));
            //curveArray.Append(Line.CreateBound(pG, pH));
            //curveArray.Append(Line.CreateBound(pH, pI));
            //curveArray.Append(Line.CreateBound(pI, pJ));
            //curveArray.Append(Line.CreateBound(pJ, pK));
            //curveArray.Append(Line.CreateBound(pK, pL));
            //curveArray.Append(Line.CreateBound(pL, pP));
            //curveArray.Append(Line.CreateBound(pP, pO));
            //curveArray.Append(Line.CreateBound(pO, pN));
            //curveArray.Append(Line.CreateBound(pN, pM));
            //curveArray.Append(Line.CreateBound(pM, pQ));
            //curveArray.Append(Line.CreateBound(pQ, pR));
            //curveArray.Append(Arc.Create(pR, pT, pU));
            //curveArray.Append(Line.CreateBound(pT, pS));
            //curveArray.Append(Line.CreateBound(pS, pA));

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
            try
            {
                using (Transaction trans = new Transaction(doc, "Creat Floor"))
                {
                    trans.Start();
                    Floor floor = doc.Create.NewFloor(curveArray, false);
                    //View activeView = doc.ActiveView;
                    //SketchPlane sketchPlane = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(activeView.ViewDirection, XYZ.Zero));
                    //foreach (Curve curve in curveArray)
                    //{
                    //    ModelCurve modelCurve = doc.Create.NewModelCurve(curve, sketchPlane);
                    //}

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
