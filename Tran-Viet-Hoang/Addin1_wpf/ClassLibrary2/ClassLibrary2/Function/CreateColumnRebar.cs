using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace CreateColumnRebar
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_00");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "19M");

            Reference col = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick");
            Element elem = doc.GetElement(col);
            Parameter elemlength = elem.LookupParameter("Length");



            double cover = 45 / 304.8;
            string elemlengthvalue = elemlength.AsValueString();

            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin1 = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);

            XYZ xVec = new XYZ(0, 0, 1); // line 2
            XYZ yVec = new XYZ(1, 0, 0);

            try
            {
                using (var transaction = new Transaction(doc, "Create rebar "))
                {
                    transaction.Start();
                    Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, elem, origin1, xVec, yVec);
                    Parameter rebarlength = rebar.LookupParameter("B");
                    double oldlength = rebarlength.AsDouble();
                    XYZ point1 = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover + oldlength / 2);
                    XYZ point2 = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover + 10, boundingbox.Min.Z + cover + oldlength / 2);
                    Line axis = Line.CreateBound(point1, point2);

                    //rebarlength.Set(3800 / 304.8);
                    rebarlength.Set(Convert.ToDouble(elemlengthvalue) / 304.8 - 2 * cover);

                    ElementTransformUtils.RotateElement(doc, rebar.Id, axis, Math.PI);

                    transaction.Commit();
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