using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace RevitAPI_Sample.Utils
{
    public static class Common
    {
        internal static bool IsLineParallel(Line line1, Line line2)
        {
            //if (line1.Direction.CrossProduct(line2.Direction) == XYZ.Zero)
            //{
            //    return true;
            //}
            //else
            //    return false;

            XYZ fistDirection = line1.Direction;
            XYZ secondDirection = line2.Direction;
            double angle = fistDirection.AngleTo(secondDirection);
            if (angle == 0 || angle == Math.PI)
            {
                return true;
            }

            return false;
        }

        internal static bool IsLineParallel(Line line1, Line line2, double angleToloranceDo)
        {
            //if (line1.Direction.CrossProduct(line2.Direction) == XYZ.Zero)
            //{
            //    return true;
            //}
            //else
            //    return false;
            var angleTolorance = angleToloranceDo * Math.PI / 180;

            XYZ fistDirection = line1.Direction;
            XYZ secondDirection = line2.Direction;
            double angle = fistDirection.AngleTo(secondDirection);
            if (Math.Abs(angle - 0) < angleTolorance || Math.Abs(angle - Math.PI) < angleTolorance)
            {
                return true;
            }

            return false;
        }

        internal static XYZ TransformPointToStandardCoordinate(Plane plane, UV originalPoint)
        {
            XYZ point = null;

            if (plane != null && originalPoint != null)
            {
                // set point as the origin of the plane in standard coordinate
                point = plane.Origin - XYZ.Zero;

                // calculate the translation vector from plane orgin
                XYZ uVec = plane.XVec.Normalize() * originalPoint.U;
                XYZ vVec = plane.YVec.Normalize() * originalPoint.V;
                XYZ translation = uVec + vVec;

                // move the point its location
                point += translation;
            }

            return point;
        }

        /// <summary>
        /// Get plane from view
        /// </summary>
        internal static Plane GetPlaneFromView(View view)
        {
            Plane plane = null;

            if (view != null && !(view is View3D))
            {
                XYZ origin = view.Origin;
                Level level = view.GenLevel;
                if (level != null)
                    origin = new XYZ(view.Origin.X, view.Origin.Y, level.ProjectElevation);
                plane = Plane.CreateByNormalAndOrigin(view.ViewDirection.Normalize(), origin);
            }

            return plane;
        }

        /// <summary>
        /// Prompt user to pick a list specific type of element
        /// </summary>
        internal static IList<ElementId> PickElements(UIDocument uiDocument, ISelectionFilter filter, string promptMessage)
        {
            IList<Reference> elemRefs;
            try
            {
                elemRefs = uiDocument.Selection.PickObjects(ObjectType.Element, filter, promptMessage);
            }
            // handle the case user baort selection
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }

            List<ElementId> elemIds = new List<ElementId>();
            foreach (var elemRef in elemRefs)
            {
                Element elem = uiDocument.Document.GetElement(elemRef.ElementId);
                elemIds.Add(elem.Id);
            }

            return elemIds;
        }

        /// <summary>
        /// Prompt user to pick a specific type of element
        /// </summary>
        internal static Element PickElement(UIDocument uIDocument, ISelectionFilter filter, string promptMessage)
        {
            Reference elemRef;
            try
            {
                elemRef = uIDocument.Selection.PickObject(ObjectType.Element, filter, promptMessage);
            }
            // handle the case user baort selection
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }

            Element element = uIDocument.Document.GetElement(elemRef.ElementId);
            return element;
        }

        /// <summary>
        /// Filter for selectionm prompt to select Beam only
        /// </summary>
        internal class DoorSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element.Category.Name.Equals("Doors");
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        internal class RoomSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element is Room;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        internal class GroupSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem is Group ? true : false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}