using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    class CreateNewFloor : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            //Create point
            XYZ p1 = new XYZ(-10, -10, 0);
            XYZ p2 = new XYZ(10, -10, 0);
            XYZ p3 = new XYZ(20, 0, 0);
            XYZ p4 = new XYZ(10, 10, 0);
            XYZ p5 = new XYZ(-10, 10, 0);

            //Create Line
            Line cur1 = Line.CreateBound(p1, p2);
            Arc cur2 = Arc.Create(p2, p4, p3);
            Line cur3 = Line.CreateBound(p4, p5);
            Line cur4 = Line.CreateBound(p5, p1);

            //Create Curve Array
            CurveArray curArray = new CurveArray();
            curArray.Append(cur1);
            curArray.Append(cur2);
            curArray.Append(cur3);
            curArray.Append(cur4);

            try
            {
                using (Transaction trans = new Transaction(doc, "Place Family"))
                {
                    trans.Start();

                    doc.Create.NewFloor(curArray, false);
                    
                    trans.Commit();
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
