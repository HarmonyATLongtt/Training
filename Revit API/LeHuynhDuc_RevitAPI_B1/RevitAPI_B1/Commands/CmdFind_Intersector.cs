using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdFind_Intersector : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                if (r != null)
                {
                    ElementId elementid = r.ElementId;
                    Element element = doc.GetElement(elementid);

                    XYZ ray = new XYZ(0, 0, 1);

                    LocationPoint locationPoint = element.Location as LocationPoint;
                    XYZ projectRay = locationPoint.Point;

                    ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Roofs);
                    ReferenceIntersector referenceIn = new ReferenceIntersector(filter, FindReferenceTarget.All, (View3D)doc.ActiveView);

                    ReferenceWithContext referContext = referenceIn.FindNearest(projectRay, ray);
                    if (referContext != null)
                    {
                        Reference reference = referContext.GetReference();
                        XYZ intPoint = reference.GlobalPoint;
                        double instance = projectRay.DistanceTo(intPoint);
                        TaskDialog.Show("Intersection", string.Format($"Diem cat {intPoint} \n Khoang cach tu goc toi diem cat la {instance}"));
                    }
                    else
                    {
                        TaskDialog.Show("Intersection", string.Format($"Khong co giao diem!!"));
                    }

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
