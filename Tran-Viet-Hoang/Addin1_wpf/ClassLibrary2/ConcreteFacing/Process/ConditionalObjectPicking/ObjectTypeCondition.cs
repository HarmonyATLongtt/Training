using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConcreteFacing.Process
{
    public class ObjectTypeCondition
    {
        public List<Element> PickConcreteBeamOrColumn(UIDocument uidoc)
        {
            try
            {
                List<Element> result = new List<Element>();
                IList<Reference> refer = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new Common.AllowBeamAndColumn(), "chon dam hoac cot be tong") as IList<Reference>;
                if (refer != null)

                    foreach (Reference r in refer)
                    {
                        result.Add(uidoc.Document.GetElement(r));
                    }
                return result;
            }
            catch (OperationCanceledException)
            {
            }
            return null;
        }

        public List<Element> SelectedConcreteBeamOrColumn(UIDocument uidoc)
        {
            try
            {
                ICollection<ElementId> result = new List<ElementId>();
                List<Element> allelem = uidoc.Selection.GetElementIds().Select(id => uidoc.Document.GetElement(id)).ToList();
                List<Element> beamandcol = new List<Element>();
                foreach (Element ele in allelem)
                {
                    FamilyInstance ins = ele as FamilyInstance;
                    if (ins != null && (ins.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column ||
                        ins.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam))
                    {
                        beamandcol.Add(ele);
                        result.Add(ele.Id);
                    }
                }

                return beamandcol;
            }
            catch (OperationCanceledException)
            {
            }
            return null;
        }
    }
}