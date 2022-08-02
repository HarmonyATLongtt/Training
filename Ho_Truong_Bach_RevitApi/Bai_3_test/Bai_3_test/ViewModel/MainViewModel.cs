using Autodesk.Revit.DB.Events;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Bai_3_test.View;
using Bai_3.Model;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Bai_3.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public MainModel _mainModel;       
        public IList<Level> ListLevel
        {
            get { return _mainModel.LevelList; }
            set { _mainModel.LevelList = value; OnPropertyChanged(); }
        }
        public IList<FamilySymbol> ListSymbol
        {
            get { return _mainModel.FamilyTypeList; }
            set { _mainModel.FamilyTypeList = value; OnPropertyChanged(); }
        }
        public Level SelectedLevel
        {
            get { return _mainModel.SelectedLevel; }
            set { _mainModel.SelectedLevel = value; OnPropertyChanged(); }
        }
        public FamilySymbol SelectedSymbol
        {
            get { return _mainModel.SelectedType; }
            set { _mainModel.SelectedType = value; OnPropertyChanged(); }
        }
        public RelayCommand OkCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public MainViewModel(MainModel model)
        {
            _mainModel = model;
            OkCommand = new RelayCommand(OK);
            CancelCommand = new RelayCommand(Cancel);
        }
        private void Cancel(object obj)
        {
            if(obj is Window window)
            {

                window.Close();
            }
        }
        private void OK(object obj)
        {
            if (obj is Window window)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
        
    }       
}
