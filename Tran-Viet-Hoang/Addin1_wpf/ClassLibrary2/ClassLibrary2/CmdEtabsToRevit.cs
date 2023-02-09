using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using ClassLibrary2.Factory.EtabDataExtractor;
using ClassLibrary2.Factory.LevelSet;
using ClassLibrary2.Factory.ReinforcingBeamSet;
using ClassLibrary2.Factory.ReinforcingColumnSet;
using ClassLibrary2.UI.ViewModel;
using ClassLibrary2.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CmdEtabsToRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                MainView view = new MainView();
                view.DataContext = new MainViewModel();
                if (view.ShowDialog() == true)
                {
                    MainViewModel vm = view.DataContext as MainViewModel;
                    var tablelist = vm.Tables.ToList();
                    EtabExtractor etabdatagetting = new EtabExtractor(tablelist);
                    List<LevelData> LevelModelData = etabdatagetting.LevelReadData();
                    List<ConcreteBeamData> BeamModelData = etabdatagetting.ExtractBeam();
                    List<ConcreteColumnData> ColumnModelData = etabdatagetting.ExtractCol();

                    new CreatingLevel().CreateLevel(commandData, LevelModelData);

                    new CreatingConcreteBeamHost().CreateBeams(doc, BeamModelData);
                    new CreatingConcreteColumnHost().CreateCols(doc, ColumnModelData, LevelModelData);

                    new CreatingColumnDesignRebar().CreateColumnRebar(doc, ColumnModelData);
                    new CreatingBeamDesignRebar().CreateBeamRebar(doc, BeamModelData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }

            return Result.Succeeded;
        }
    }
}