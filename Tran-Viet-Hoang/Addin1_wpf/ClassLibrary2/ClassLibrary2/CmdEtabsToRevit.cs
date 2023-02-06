using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClassLibrary2.Data;
using ClassLibrary2.Factory.EtabDataExtractor;
using ClassLibrary2.Factory.RebarSet;
using ClassLibrary2.Function;
using ClassLibrary2.UI.ViewModel;
using ClassLibrary2.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
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
                if (view.ShowDialog() == true)
                {
                    MainViewModel vm = view.DataContext as MainViewModel;
                    var tablelist = vm.Tables.ToList();
                    EtabExtractor etabdatagetting = new EtabExtractor(tablelist);
                    List<LevelData> LevelModelData = etabdatagetting.LevelReadData();
                    List<ConcreteBeamData> BeamModelData = etabdatagetting.ExtractBeam();
                    List<ConcreteColumnData> ColumnModelData = etabdatagetting.ExtractCol();

                    //tạo hệ thống level
                    foreach (LevelData levelData in LevelModelData)
                    {
                        new Remodel_CreateLevel().CreateLevel(commandData, levelData.Name, levelData.Elevation);
                    }

                    //vẽ dầm kèm set rebarcover
                    new Remodel_CreateBeam().CreateBeams(doc, BeamModelData);
                    new Remodel_CreateColumn().CreateCols(doc, ColumnModelData, LevelModelData);

                    // tinhh toan rebar
                    RebarSet rebarFactory = new RebarSet(doc);
                    foreach (var beam in BeamModelData)
                        beam.Rebars = rebarFactory.CalculateBeamRebar(beam);

                    foreach (var col in ColumnModelData)
                        col.Rebars = rebarFactory.CalculateColumnRebar(col);

                    //vẽ 1 stirrup ban đầu cho cột và dầm đồng thời set lại giá trị cho stirrup đó để phù hợp với kích thước cấu kiện
                    new Remodel_SetColumnStirrup().drawcolstirrup(doc, ColumnModelData);
                    new Remodel_SetBeamStirrup().drawbeamstirrup(doc, BeamModelData);

                    //sau khi set giá trị mới cho stirrup thì move stirrup về nằm gọn trong cấu kiện
                    new Remodel_MoveStirrup().MoveStirrup(doc, ColumnModelData, BeamModelData);

                    new Remodel_SetBeamStandard().SetAllBeamStandard(doc, BeamModelData);
                    MessageBox.Show("Have a greate day");
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