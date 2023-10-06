using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitTrainees.Utils;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_EditAndReloadFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, "Pick object to edit family...");
            Element element = doc.GetElement(reference);

            Family family = null;

            if (element is FamilyInstance instance)
            {
                family = instance.Symbol.Family;
            }

            try
            {
                Document familyDocument = doc.EditFamily(family);

                var extrusion = new FilteredElementCollector(familyDocument)
                    .WhereElementIsNotElementType()
                    .Cast<Element>()
                    .FirstOrDefault(e => e is Extrusion);

                using (var trans = new Transaction(familyDocument, "Edit family and reload"))
                {
                    trans.Start();

                    ElementTransformUtils.MoveElement(familyDocument, new ElementId(extrusion.Id.IntegerValue), new XYZ(-15, 0, 0));
                    ElementTransformUtils.CopyElement(familyDocument, new ElementId(extrusion.Id.IntegerValue), new XYZ(25, 0, 0));
                    familyDocument.LoadFamily(doc, new FamilyOptions());

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}