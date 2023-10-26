using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Data.Common;
using Document = Autodesk.Revit.DB.Document;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdMove_Column : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Reference r = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            //FamilyInstance instane = 
            using(Transaction trans = new Transaction(doc, "Move_Column"))
            {
                trans.Start();
                //MoveColumm(doc, instance);
                trans.Commit();
            }
            return Result.Succeeded;
        }

        public void MoveColumm(Document doc, FamilyInstance column)
        {
            LocationPoint columnLocation = column.Location as LocationPoint;
            XYZ oldPlace = columnLocation.Point;

            // Move the column to new location.
            XYZ newPlace = new XYZ(10, 20, 30);
            ElementTransformUtils.MoveElement(doc, column.Id, newPlace);

            // now get the column's new location
            columnLocation = column.Location as LocationPoint;
            XYZ newActual = columnLocation.Point;

            string info = "Original Z location: " + oldPlace.Z +
                            "\nNew Z location: " + newActual.Z;

            TaskDialog.Show("Revit", info);
        }
    }
}
