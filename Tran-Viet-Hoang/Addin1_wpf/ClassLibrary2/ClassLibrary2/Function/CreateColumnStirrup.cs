using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;

using System.Linq;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace CreateColumnStirrup
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
                .First(x => x.Name == "M_T1");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "8M");

            Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick");
            FamilyInstance columnn = doc.GetElement(reference) as FamilyInstance;
            Parameter elemlength = columnn.LookupParameter("Length");
            Parameter col_b = columnn.Symbol.LookupParameter("b");
            Parameter col_h = columnn.Symbol.LookupParameter("h");

            //Location loc = columnn.Location;

            //khai báo giá trị other cover, để xác định chính xác length của thép
            double cover = 50 / 304.8;

            BoundingBoxXYZ boundingbox = columnn.get_BoundingBox(null);
            XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
            XYZ max = boundingbox.Transform.OfPoint(boundingbox.Max);

            XYZ origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);

            XYZ yVec = new XYZ(0, 1, 0);
            XYZ xVec = new XYZ(1, 0, 0);
            if (Convert.ToDouble(boundingbox.Max.X - boundingbox.Min.X) > Convert.ToDouble(boundingbox.Max.Y - boundingbox.Min.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Max.Z - cover);
                xVec = new XYZ(0, 1, 0);
                yVec = new XYZ(1, 0, 0);
            }

            try
            {
                using (var transaction = new Transaction(doc, "Create stirrup "))
                {
                    transaction.Start();

                    Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, columnn, origin, xVec, yVec);

                    Parameter tie_B = rebar.LookupParameter("B");
                    Parameter tie_C = rebar.LookupParameter("C");
                    Parameter tie_D = rebar.LookupParameter("D");
                    Parameter tie_E = rebar.LookupParameter("E");
                    MessageBox.Show(xVec + "\n" + yVec);

                    double B_D = col_b.AsDouble() - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = col_h.AsDouble() - 2 * cover;
                    tie_C.Set(C_E);
                    tie_E.Set(C_E);

                    transaction.Commit();

                    transaction.Start();
                    BoundingBoxXYZ boundingboxnew = rebar.get_BoundingBox(null);
                    XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin - origin1;

                    ElementTransformUtils.MoveElement(doc, rebar.Id, vect);

                    rebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, elemlength.AsDouble(), true, true, false);

                    transaction.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
                return Result.Failed;
            }
        }
    }
}