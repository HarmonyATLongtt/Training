using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BaiTap_Revit.P5
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Dimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference ref1 = uidoc.Selection.PickObject(ObjectType.Element, "Select the first wall");
                Reference ref2 = uidoc.Selection.PickObject(ObjectType.Element, "Select the second wall");

                Element wall1 = doc.GetElement(ref1);
                Element wall2 = doc.GetElement(ref2);
                LocationCurve locCurve1 = wall1.Location as LocationCurve;
                LocationCurve locCurve2 = wall2.Location as LocationCurve;
                if (locCurve1 != null && locCurve2 != null)
                {
                    using (Transaction tx = new Transaction(doc, "Create Dimension"))
                    {
                        tx.Start();

                        // Create a reference array for the dimension
                        ReferenceArray refArray = new ReferenceArray();
                        refArray.Append(ref1);
                        refArray.Append(ref2);

                        // Create the dimension line
                        Line dimLine = Line.CreateBound(locCurve1.Curve.GetEndPoint(0), locCurve2.Curve.GetEndPoint(0));

                        // Create the dimension
                        Autodesk.Revit.DB.Dimension dimension = doc.Create.NewDimension(doc.ActiveView, dimLine, refArray);

                        tx.Commit();
                    }
                }
                else
                {
                    message = "Failed to get the location curves of the selected walls.";
                    return Result.Failed;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}