using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using ClassLibrary2.UI.Views;
using ClassLibrary2.UI.ViewModel;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.UI.Selection;

namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MainView view = new MainView();
            view.DataContext = new MainViewModel();
            view.Show();

            //UIDocument uidoc = commandData.Application.ActiveUIDocument;
            //Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            //try
            //{
            //    Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            //    if (r != null)
            //    {                 
            //        TaskDialog.Show("Element Id", r.ElementId.ToString());                  
            //    }
            //    return Result.Succeeded;
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //    return Result.Failed;
            //}

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            //Reference pickObject = uidoc.Selection.PickObject(ObjectType.Element, new Filtertest());
            //Element element = doc.GetElement(pickObject);
            //MessageBox.Show(element.Category.Name);

            return Result.Succeeded;
        }
    }
}
