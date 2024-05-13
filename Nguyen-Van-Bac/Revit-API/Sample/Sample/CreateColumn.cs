using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            XYZ origin = uiDoc.Selection.PickPoint();

            FilteredElementCollector collector = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID).OfClass(typeof(Level));
            Level firstLevel = collector.FirstElement() as Level;
            FilteredElementCollector colColumns = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Columns);
            FamilySymbol firstColumn = colColumns.FirstElement() as FamilySymbol;
            try
            {
                using (Transaction tx = new Transaction(doc, "add column"))
                {
                    tx.Start();
                    if (!firstColumn.IsActive)
                    {
                        firstColumn.Activate();
                    }
                    //Create Coulmn

                    doc.Create.NewFamilyInstance(origin, firstColumn, firstLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                    tx.Commit();
                }
            }
            catch
            {
                message = "Unexpected Exception thrown.";
                return Autodesk.Revit.UI.Result.Failed;
            }
            TaskDialog.Show("Đã thêm Cot", "ok");
            return Result.Succeeded;
        }
    }
}