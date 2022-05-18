using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bai_3.Model;
using Bai_3_test.View;
using System;
using System.Windows.Forms;

namespace Bai_3.ViewModel
{
    [Transaction(TransactionMode.Manual)]
    public class OpenCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (TransactionGroup group = new TransactionGroup(doc, "Load family"))
            {
                try
                {
                    group.Start();
                    MainModel mainModel = new MainModel(doc);
                    MainViewModel mainViewModel = new MainViewModel(mainModel);

                    if (mainModel._Family != null)
                    {
                        LoadFamilyView mainView = new LoadFamilyView();
                        mainView.DataContext = mainViewModel;

                        if (mainView.ShowDialog() == true)
                        {
                            if (mainViewModel.SelectedSymbol != null && mainViewModel.SelectedLevel != null)
                            {
                                mainModel.LoadedFamilyInstance(mainViewModel.SelectedSymbol, doc);
                                group.Assimilate();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
                }
                finally
                {
                    if (group.HasStarted() && group.GetStatus() != TransactionStatus.Committed)
                        group.RollBack();
                }
            }
            return Result.Succeeded;
        }
    }
}