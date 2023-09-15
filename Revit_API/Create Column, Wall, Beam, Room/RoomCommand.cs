using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateColumnApi
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Selection selection = uiDoc.Selection;
            IList<Reference> referenceList = new List<Reference>();

            try
            {
                referenceList = selection.PickObjects(ObjectType.Element, new ElemFilter(e => e is Wall), "Select four wall create closed shape!").ToList();
            }
            catch
            {

            }

            List<Element> elementsList = new List<Element>();
            foreach (var rl in referenceList)
            {
                elementsList.Add(doc.GetElement(rl));
            }



            using (var transaction = new Transaction(doc, "Create Room"))
            {
                transaction.Start();

                try
                {
                    //if(AreWallFormingClosedShape(doc, elementsList))
                    //{
                    float x = 0;
                    foreach (var i in elementsList)
                    {
                        x += (float)((i.Location as LocationCurve).Curve as Line).Origin.X;
                        TaskDialog.Show(",", x.ToString());
                    }
                    x /= 4;

                    float y = 0;
                    foreach (var i in elementsList)
                    {
                        y += (float)((i.Location as LocationCurve).Curve as Line).Origin.Y;
                    }
                    y /= 4;

                    UV point = new UV(x, y);

                    Room room = doc.Create.NewRoom(doc.ActiveView.GenLevel, point);
                    room.Name = "Room Create";
                    room.Number = "Number 1";
                    //}


                }
                catch (Exception ex)
                {
                    //TaskDialog.Show(",", ex.Message);
                }

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        public bool AreWallFormingClosedShape(Document doc, List<Element> elems)
        {
            if (elems.Count != 4)
                return false;

            List<Curve> wallCurves = new List<Curve>();

            foreach (Element e in elems)
            {
                if (e is Wall wall)
                {
                    LocationCurve locationCurve = e.Location as LocationCurve;
                    if (locationCurve != null)
                        wallCurves.Add(locationCurve.Curve);
                }
            }

            if (wallCurves.Count != 4)
            {
                return false;
            }

            CurveLoop curveLoop = CurveLoop.Create(wallCurves);

            return curveLoop != null && !curveLoop.IsOpen();
        }
    }

    public class ElemFilter : ISelectionFilter
    {
        private readonly Func<Element, bool> validateElement;

        public ElemFilter(Func<Element, bool> validateElement)
        {
            this.validateElement = validateElement;
        }
        public bool AllowElement(Element elem)
        {
            return validateElement(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
