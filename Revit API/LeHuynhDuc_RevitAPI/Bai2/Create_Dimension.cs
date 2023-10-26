using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    public class Create_Dimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Pick_Line(doc, uidoc);
            return Result.Succeeded;
        }
        public static XYZ FindProjectionPointOnLine(XYZ point, Line line)
        {
            XYZ projection = line.Project(point).XYZPoint;
            return projection;
        }
        public void Pick_Line(Document doc, UIDocument uidoc)
        {
            Application app = doc.Application;

            //first create two lines

            XYZ pt11 = uidoc.Selection.PickPoint();
            XYZ pt12 = uidoc.Selection.PickPoint();
            Line line = Line.CreateBound(pt11, pt12);

            XYZ pt21 = uidoc.Selection.PickPoint();
            XYZ pt22 = uidoc.Selection.PickPoint();
            Line line1 = Line.CreateBound(pt21, pt22);

            //XYZ pt11 = new XYZ(5, 50, 0);
            //XYZ pt12 = new XYZ(5, 10, 0);
            //Line line = Line.CreateBound(pt11, pt12);

            //XYZ pt21 = new XYZ(10, 5, 0);
            //XYZ pt22 = new XYZ(10, 10, 0);
            //Line line1 = Line.CreateBound(pt21, pt22);

            using (Transaction trans = new Transaction(doc, "create_line"))
            {
                trans.Start();
                Plane plane = Plane.CreateByNormalAndOrigin(uidoc.ActiveView.ViewDirection, pt11);
                SketchPlane skplane = SketchPlane.Create(doc, plane);
                Plane plane1 = Plane.CreateByNormalAndOrigin(uidoc.ActiveView.ViewDirection, pt21);
                SketchPlane skplane1 = SketchPlane.Create(doc, plane1);
                ModelCurve modelCurve = doc.Create.NewModelCurve(line,skplane);
                ModelCurve modelCurve1 = doc.Create.NewModelCurve(line1, skplane1);


                ReferenceArray ra = new ReferenceArray();
                ra.Append(modelCurve.GeometryCurve.Reference);
                ra.Append(modelCurve1.GeometryCurve.Reference);

                //XYZ pt1 = new XYZ(5, 10, 0);
                //XYZ pt2 = new XYZ(10, 10, 0);
                XYZ pt1 = new XYZ();
                XYZ pt2 = new XYZ();

                pt1 = pt11;
                //hình chiếu của pt1 lên đường thẳng line1
                pt2 = FindProjectionPointOnLine(pt1, line1);

                Line line2 = Line.CreateBound(pt1, pt2);
                Dimension dim = doc.Create.NewDimension(doc.ActiveView, line, ra);

                trans.Commit();
            }
            // now create a linear dimension between them

            //ReferenceArray ra = new ReferenceArray();
            //ra.Append(modelcurve1.GeometryCurve.Reference);
            //ra.Append(modelcurve2.GeometryCurve.Reference);

            //pt1 = new XYZ(5, 10, 0);
            //pt2 = new XYZ(10, 10, 0);
            //Line line2 = Line.CreateBound(pt1, pt2);
            //Dimension dim = doc.FamilyCreate
            //  .NewLinearDimension(doc.ActiveView, line, ra);

            //// create a label for the dimension called "width"

            //FamilyParameter param = doc.FamilyManager
            //  .AddParameter("width",
            //    BuiltInParameterGroup.PG_CONSTRAINTS,
            //    ParameterType.Length, false);

            
        }
    }
}
