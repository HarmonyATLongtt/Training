using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ExRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    class CreateGrids : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            using (Transaction transaction = new Transaction(doc, "Create Gird"))
            {
                transaction.Start();
                try
                {
                    // Create the geometry line which the grid locates
                    XYZ start = new XYZ(0, 0, 0);
                    XYZ end = new XYZ(0, 50, 0);

                    XYZ startB = new XYZ(-50, 25, 0);
                    XYZ endB = new XYZ(0, 0, 0);
                    Line geomLine = Line.CreateBound(start, end);
                    Line geomLineB = Line.CreateBound(startB, endB);

                    // Create a grid using the geometry line
                    Grid lineGrid = Grid.Create(doc, geomLine);
                    Grid lineGrid2 = Grid.Create(doc, geomLineB);

                    if (null == lineGrid)
                    {
                        throw new Exception("Create a new straight grid failed.");
                    }

                    // Modify the name of the created grid
                    lineGrid.Name = "A";
                    lineGrid2.Name = "B";

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return Result.Succeeded;
        }
    }
}
