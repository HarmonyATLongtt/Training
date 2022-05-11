using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bai_3.Model;
using Bai_3.ViewModel;
using Bai_3_test.View;
using System.Windows;

namespace Bai_3.ViewModel
{
    [Transaction(TransactionMode.Manual)]
    public class OpenCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            { 
                MainModel mainModel = new MainModel(doc);
                MainViewModel mainViewModel = new MainViewModel(mainModel);
                LoadFamilyView mainView = new LoadFamilyView();
                mainView.DataContext = mainViewModel;
                
                if (mainView.ShowDialog() == true)
                {               
                    if (mainViewModel.SelectedSymbol != null && mainViewModel.SelectedLevel != null)
                    {
                        mainModel.LoadedFamilyInstance(mainViewModel.SelectedSymbol, doc);
                    }
                }       
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}

