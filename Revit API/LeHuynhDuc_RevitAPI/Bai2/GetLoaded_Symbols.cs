using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    public class GetLoaded_Symbols : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Chọn một FamilyInstance");

            // Lấy đối tượng FamilyInstance từ Reference
            FamilyInstance familyInstance = doc.GetElement(pickedRef.ElementId) as FamilyInstance;
           
            GetLoadedSymbols(doc, familyInstance);
            
            return Result.Succeeded;
        }
        public void GetLoadedSymbols(Document document, FamilyInstance familyInstance)
        {
            if (null != familyInstance.Symbol)
            {
                Family family = familyInstance.Symbol.Family;
                Document familyDoc = document.EditFamily(family);
                
                using (Transaction trans = new Transaction(document, "GET_Familysymbol"))
                {
                    trans.Start();

                    if (familyDoc != null && familyDoc.IsFamilyDocument==true)
                    {
                        string loadedFamilies = "FamilySymbols in " + family.Name + ":\n";
                        FilteredElementCollector collector = new FilteredElementCollector(document);
                        collector.OfClass(typeof(FamilySymbol));

                        // Lọc các FamilySymbol thuộc về Family có tên nhất định
                        ICollection<FamilySymbol> collection = collector
                            .WhereElementIsElementType()
                            .Cast<FamilySymbol>()
                            .Where(s => s.FamilyName == family.Name).ToList();

                        foreach (Element e in collection)
                        {
                            FamilySymbol fs = e as FamilySymbol;
                            loadedFamilies += "\t" + fs.Name + "\n";
                        }

                        TaskDialog.Show("Revit", loadedFamilies);
                    }
                    trans.Commit();
                }
            }
        }
    }
}
