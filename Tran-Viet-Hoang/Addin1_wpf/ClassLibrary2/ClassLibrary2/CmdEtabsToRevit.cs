using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using ClassLibrary2.UI.ViewModel;
using ClassLibrary2.UI.Views;
using System;
using System.Collections.Generic;
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
                List<LevelData> LevelModelData = vm.LevelDatas;

                //foreach (LevelData levelData in LevelModelData)
                //{
                //    MessageBox.Show(levelData.Name + " " + levelData.Elevation);
                //}

                foreach (LevelData levelData in LevelModelData)
                {
                    LevelModel(commandData, levelData.Name, levelData.Elevation);
                }
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
            Document doc = uidoc.Document;
            var elems = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels);
            string Levelexisted = "false";

            using (var transaction = new Transaction(doc, "Set Elevation"))
            {
                foreach (var singlelevel in elems)
                {
                    Parameter para = singlelevel.LookupParameter("Name");
                    Parameter cote = singlelevel.LookupParameter("Elevation");
                    if (para.AsString() == uniquename)
                    {
                        transaction.Start();
                        Levelexisted = "true";
                        cote.Set(elevation / 304.8);
                        transaction.Commit();
                    }
                }

                if (Levelexisted == "false")
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