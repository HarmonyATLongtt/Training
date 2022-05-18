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
        /// <summary>
        /// Prompt user to pick a list specific type of element
        /// </summary>
        internal static IList<Element> PickElements(UIDocument uiDocument, ISelectionFilter selectionFilter, string promptMessage)
        {
            IList<Reference> elemRefs;
            try
            {
                elemRefs = uiDocument.Selection.PickObjects(ObjectType.Element, selectionFilter, promptMessage);
                List<Element> elems = new List<Element>();
                foreach (var elemRef in elemRefs)
                {
                    Element elem = uiDocument.Document.GetElement(elemRef.ElementId);
                    elems.Add(elem);
                }
                return elems;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return new List<Element>();
            }
        }
    }

    internal class FrammingElementFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem != null ? elem.Category?.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming : false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}