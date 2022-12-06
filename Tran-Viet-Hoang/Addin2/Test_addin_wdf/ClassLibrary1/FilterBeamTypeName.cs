using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using ClassLibrary1.UI.ViewModel;
using ClassLibrary1.UI.Views;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows;
using ClassLibrary1;
using System.Data;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB.Structure;

namespace FilterBeamTypeName
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            //DataTable TableBeam = new DataTable
            //Reference r = uidoc;

            //DataTable TableBeam = new DataTable(); 
            //TableBeam.Columns.Add("Type");

            //filter vật liệu
            FamilyStructuralMaterialTypeFilter filterM = new FamilyStructuralMaterialTypeFilter(StructuralMaterialType.Concrete);
            //filter dầm
            ElementCategoryFilter filterB = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming,true);
            LogicalAndFilter logic = new LogicalAndFilter(filterB, filterM);

            IList<Element> filterlogic = new FilteredElementCollector(doc).WherePasses(logic).ToElements();
               
            string mess = "";
            foreach (Element eachbeam in filterlogic)
            {
                if(null == eachbeam.GetTypeId())
                {
                    TaskDialog.Show("Revit", "Not found");
                }
                else 
                {
                    mess = eachbeam.Name;
                    TaskDialog.Show("Revit", mess);
                }
            }
          
            //TaskDialog.Show("Revit", mess);
            return Result.Succeeded;
        }                     
    }
}
