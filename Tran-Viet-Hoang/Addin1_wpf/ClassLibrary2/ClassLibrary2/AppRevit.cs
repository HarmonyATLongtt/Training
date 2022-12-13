using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary2.UI.ViewModel;
using ClassLibrary2.UI.Views;
using System;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class AppRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
           
         
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                MainView view = new MainView();
                view.DataContext = new MainViewModel();
                view.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }

            return Result.Succeeded;
        }
    }
}
