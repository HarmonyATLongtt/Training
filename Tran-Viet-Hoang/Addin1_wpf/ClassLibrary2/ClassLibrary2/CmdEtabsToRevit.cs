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
    public class CmdEtabsToRevit : IExternalCommand
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

                MainViewModel vm = view.DataContext as MainViewModel;
                //vm.LevelDatas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }

            return Result.Succeeded;
        }

        public void LevelModel(ExternalCommandData commandData, string uniquename, double elevation)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            var elems = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels);
            foreach (var name in elems)
            {
                Parameter para = name.LookupParameter("Name");
                Parameter cote = name.LookupParameter("Elevation");
                if (para.AsString() == uniquename)
                {
                    using (var transaction = new Transaction(doc, "Set Elevation"))
                    {
                        transaction.Start();
                        cote.Set(elevation / 304.8);
                        transaction.Commit();
                    }
                }
                else
                {
                    using (var transaction = new Transaction(doc, "Create Level"))
                    {
                        transaction.Start();
                        Level newlevel = Level.Create(doc, elevation);
                        Parameter paranew = newlevel.LookupParameter("Name");
                        paranew.Set(uniquename);
                        transaction.Commit();
                    }
                }
            }
        }
    }
}