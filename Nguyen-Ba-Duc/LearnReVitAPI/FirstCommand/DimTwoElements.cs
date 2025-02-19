using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace FirstCommand
{
    [Transaction(TransactionMode.Manual)]
    public class DimTwoElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            Reference pickRef1 = uiapp.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Select wall1 to dimension");
            Element selectedElem1 = doc.GetElement(pickRef1);

            Reference pickRef2 = uiapp.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Select wall2 to dimension");
            Element selectedElem2 = doc.GetElement(pickRef2);

            if (selectedElem1 is Wall && selectedElem2 is Wall)
            {
                Wall selectedWall1 = selectedElem1 as Wall;
                Wall selectedWall2 = selectedElem2 as Wall;

                LocationCurve locWall1 = selectedWall1.Location as LocationCurve;
                Line lineWall1 = locWall1.Curve as Line;

                LocationCurve locWall2 = selectedWall2.Location as LocationCurve;
                Line lineWall2 = locWall2.Curve as Line;

                ReferenceArray referenceArray = new ReferenceArray();
                referenceArray.Append(pickRef1);
                referenceArray.Append(pickRef2);

                XYZ point1 = lineWall1.GetEndPoint(0);
                XYZ point2 = lineWall2.Project(point1).XYZPoint;

                //Line dimLine = Line.CreateBound(lineWall1.GetEndPoint(0), lineWall2.GetEndPoint(0));
                Line dimLine = Line.CreateBound(point1, point2);

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Create new dimension");

                    Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, referenceArray);

                    t.Commit();
                }
            }
            else
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}