using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Utils;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_LoadFamily : IExternalCommand, IFamilyLoadOptions
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                using (var trans = new Transaction(doc, "Load family and place an instance"))
                {
                    trans.Start();

                    Family family = new LoadFamily().Load(doc);
                    FamilySymbol symbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                    if (!symbol.IsActive)
                        symbol.Activate();

                    XYZ location = uiDoc.Selection.PickPoint("Pick location to place new instance...");

                    doc.Create.NewFamilyInstance(location, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }

        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            throw new System.NotImplementedException();
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            throw new System.NotImplementedException();
        }
    }
}