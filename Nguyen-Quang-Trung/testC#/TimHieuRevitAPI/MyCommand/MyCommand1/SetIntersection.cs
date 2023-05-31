using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyCommand1
{
    [TransactionAttribute(TransactionMode.Manual)] // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class SetIntersection : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;
            try
            {
                // get reference element
                Reference refer = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (refer != null)
                {
                    // get element
                    ElementId elementId = refer.ElementId;
                    Element element = doc.GetElement(elementId);

                    // ray
                    XYZ ray = new XYZ(0, 0, 1);

                    // project ray
                    LocationPoint locPoint = element.Location as LocationPoint;
                    XYZ projectRay = locPoint.Point;

                    ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Roofs);
                    ReferenceIntersector refInter = new ReferenceIntersector(filter, FindReferenceTarget.Face, (View3D)doc.ActiveView);

                    ReferenceWithContext refContext = refInter.FindNearest(projectRay, ray);
                    Reference reference = refContext.GetReference();

                    XYZ intPoint = reference.GlobalPoint;

                    double distance = projectRay.DistanceTo(intPoint);

                    TaskDialog.Show("Intersection", string.Format("Diem cat tu cot vao mai la {0}, khoang cach giua location point cua cot va diem cat la {1}", intPoint, distance));
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}