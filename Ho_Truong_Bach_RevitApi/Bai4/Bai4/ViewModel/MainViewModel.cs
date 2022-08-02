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
using Bai4.View;
using System.Collections.ObjectModel;
using Bai4.Model;
using System.Windows;
using System.Windows.Input;

namespace Bai4.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private MainModel _mainModel;
        public bool CubeSelect
        {
            get { return _mainModel.CubeSelect; }
            set { _mainModel.CubeSelect = value; OnPropertyChanged(); }
        }
        public bool SphereSelect
        {
            get { return _mainModel.SphereSelect; }
            set { _mainModel.SphereSelect = value; OnPropertyChanged(); }
        }
        public double CubeLength
        {
            get { return _mainModel.CubeLength; }
            set { _mainModel.CubeLength = value; OnPropertyChanged(); }
        }
        public double SphereRadius
        {
            get { return _mainModel.SphereRadius; }
            set { _mainModel.SphereRadius = value; OnPropertyChanged(); }
        }
        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public MainViewModel(MainModel model)
        {
            _mainModel = model;
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);            
        }
        private void Ok(object obj)
        {
            if (obj is Window window && Validate())
            {
                window.DialogResult = true;
                window.Close();
            }
        }
        private void Cancel(object obj)
        {
            if (obj is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
        public bool Validate()
        {
            if (CubeSelect)
            {
                return CubeLength.GetType().Equals(typeof(double)) && CubeLength > 0;
            }
            else
            {
                return SphereRadius.GetType().Equals(typeof(double)) && SphereRadius > 0;
            }
        }

    }
}

