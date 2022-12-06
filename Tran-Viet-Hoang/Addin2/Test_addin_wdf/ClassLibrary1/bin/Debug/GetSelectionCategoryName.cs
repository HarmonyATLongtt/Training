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

namespace GetSelectionCategoryName
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

            Reference pickObject = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Element element = doc.GetElement(pickObject);
            MessageBox.Show("Bạn đã chọn Elenment có category là " + element.Category.Name);
            return Result.Succeeded;
        }
    }
}
