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

namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class AppRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MainView view = new MainView();
            view.DataContext = new MainViewModel();
            view.Show();
         
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            return Result.Succeeded;
        }
    }
}
