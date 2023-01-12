using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;

using System.Linq;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace CreateBeamStirrup
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
                .First(x => x.Name == "13M");

            Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick");
            FamilyInstance beam = doc.GetElement(reference) as FamilyInstance;
            Parameter elemlength = beam.LookupParameter("Length");
            Parameter beam_b = beam.Symbol.LookupParameter("b");
            Parameter beam_h = beam.Symbol.LookupParameter("h");

            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = beam.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;
            XYZ xVec = locline.Direction; // để lấy được chiều vẽ của dầm
            //khai báo giá trị other cover, để xác định chính xác length của thép
            double cover = 50 / 304.8;
            //lấy giá trị length của cấu kiện
            string elemlengthvalue = elemlength.AsValueString();

            BoundingBoxXYZ boundingbox = beam.get_BoundingBox(null);
            XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
            XYZ max = boundingbox.Transform.OfPoint(boundingbox.Max);


            XYZ origin = XYZ.Zero;
            XYZ yVec = new XYZ(0, 0, 1);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);
                xVec = new XYZ(0, 1, 0);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Max.Y - cover, boundingbox.Min.Z + cover);
                xVec = new XYZ(1, 0, 0);
            }

            try
            {
                using (var transaction = new Transaction(doc, "Create rebar "))
                {
                    transaction.Start();

                    Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, beam, origin, xVec, yVec);

                    Parameter tie_B = rebar.LookupParameter("B");
                    Parameter tie_C = rebar.LookupParameter("C");
                    Parameter tie_D = rebar.LookupParameter("D");
                    Parameter tie_E = rebar.LookupParameter("E");
                    MessageBox.Show(xVec + "\n" + yVec);

                    //var elems = new FilteredElementCollector(doc)
                    //            .WhereElementIsNotElementType()
                    //            .OfCategory(BuiltInCategory.OST_Rebar)
                    //            .OfClass(typeof(Rebar))
                    //            .Cast<Rebar>()
                    //            .Select(x => x.Name);
                    //string a = "";
                    //foreach(var elem in elems) { a += elem; }
                    //MessageBox.Show(a);

                    //hướng dẫn Hiền
                    //Rebar shape1 = new FilteredElementCollector(doc)
                    //.OfClass(typeof(Rebar))
                    //.Cast<Rebar>()
                    //.First(x => x.GetHostId() == beam.Id);
                    //MessageBox.Show("Hoang " + shape1.Id);
                    //end hướng dẫn Hiền


                    double B_D = beam_b.AsDouble() - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = beam_h.AsDouble() - 2 * cover;
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