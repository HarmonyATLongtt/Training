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

namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MainView view = new MainView();
            //view.DataContext = new MainViewModel();
            view.Show();

            return Result.Succeeded;
        }
    }
}
