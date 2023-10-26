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
    internal class CmdCreate_Grid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            using (Transaction trans = new Transaction(doc, "Create Grid"))
            {
                trans.Start();
                CreateGrid(doc, uiDoc);
                trans.Commit();
            }
            return Result.Succeeded;
        }
        void CreateGrid(Autodesk.Revit.DB.Document document, UIDocument uiDoc)
        {
            // Create the geometry line which the grid locates
            XYZ start = uiDoc.Selection.PickPoint("Pick Start Poin");
            XYZ end = uiDoc.Selection.PickPoint("Pick End Poin");
            Line geomLine = Line.CreateBound(start, end);

            // Create a grid using the geometry line
            Grid lineGrid = Grid.Create(document, geomLine);

            if (null == lineGrid)
            {
                throw new Exception("Create a new straight grid failed.");
            }

            // Modify the name of the created grid
            // lineGrid.Name = "Grid_1";

            //// Create the geometry arc which the grid locates
            //XYZ end0 = new XYZ(0, 0, 0);
            //XYZ end1 = new XYZ(10, 40, 0);
            //XYZ pointOnCurve = new XYZ(5, 7, 0);
            //Arc geomArc = Arc.Create(end0, end1, pointOnCurve);

            //// Create a grid using the geometry arc
            //Grid arcGrid = Grid.Create(document, geomArc);

            //if (null == arcGrid)
            //{
            //    throw new Exception("Create a new curved grid failed.");
            //}

            //// Modify the name of the created grid
            //arcGrid.Name = "New Name2";
        }
    }
}
