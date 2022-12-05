using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows;
using System.Data;
using Autodesk.Revit.DB.Structure;

namespace Filter_Count_ShowName
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Collect cấu kiện có vật liệu là bê tông
            // Use FamilyStructuralMaterialType filter to find families whose material type is Wood
            
            //FamilyStructuralMaterialTypeFilter filter = new FamilyStructuralMaterialTypeFilter(StructuralMaterialType.Concrete);

            //// Apply the filter to the elements in the active document
            //FilteredElementCollector collector = new FilteredElementCollector(doc);
            //ICollection<Element> ConcreteFamiles = collector.WherePasses(filter).ToElements();

            var elems = new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .OfCategory(BuiltInCategory.OST_StructuralColumns)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .Where(x => x.IsActive)
                        .Select(x => x.Name);
            string s = "";
            foreach (var name in elems)
                s += name + "\n";
            MessageBox.Show(s);





            var e = new FilteredElementCollector(doc, uidoc.ActiveGraphicalView.Id).OfClass(typeof(FamilyInstance));

            var col = e.OfCategory(BuiltInCategory.OST_StructuralColumns).ToList();
            var beam = e.OfCategory(BuiltInCategory.OST_StructuralFraming).ToList();

            MessageBox.Show(beam.Count().ToString() + "\n" + col.Count().ToString());

           

            //uidoc.Selection.SetElementIds(elems.Select(x => x.Id).ToList());

            //string mess = "";
            //foreach (Element eachbeam in ConcreteFamiles)
            //{
            //    if(null == eachbeam.GetTypeId())
            //    {
            //        TaskDialog.Show("Revit", "Not found");
            //    }
            //    else 
            //    {
            //        Element elemtype = doc.GetElement(eachbeam.GetTypeId());
            //        mess += eachbeam.Name;
            //    }
            //}
            //TaskDialog.Show("Revit", mess);
            return Result.Succeeded;
        }       
    }
}
