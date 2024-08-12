using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.Commands.ExternalEventHandler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    public class CreatNewInstanceVM : INotifyPropertyChanged
    {
       
        private UIDocument uidoc;
        public UIDocument Uidoc { get => uidoc; set => uidoc = value; }

        private Document doc;
        public Document Doc { get => doc; set => doc = value; }

        private ObservableCollection<Level> lstLevel;

        public ObservableCollection<Level> LstLevel
        {
            get => lstLevel;
            set
            {
                lstLevel = value;
                OnPropertyChanged(nameof(Level));
            }
        }

        private Level selectedLevel;

        public Level SelectedLevel
        {
            get => selectedLevel;
            set
            {
                selectedLevel = value;
                OnPropertyChanged(nameof(SelectedLevel));
            }
        }

        private Category selectedCategory;

        public Category SelectedCategory
        {
            get => selectedCategory;
            set
            {
                selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                LoadFamilySymbol(SelectedCategory);
            }
        }

        private ObservableCollection<Category> lstCategory;

        public ObservableCollection<Category> LstCategory
        {
            get => lstCategory;
            set
            {
                lstCategory = value;
                OnPropertyChanged(nameof(LstCategory));
            }
        }

        private Object selectedFamilySymbol;

        public Object SelectedFamilySymbol
        {
            get => selectedFamilySymbol;
            set
            {
                selectedFamilySymbol = value;
                OnPropertyChanged(nameof(SelectedFamilySymbol));
            }
        }

        private ObservableCollection<Object> lstFamilySymbol;

        public ObservableCollection<Object> LstFamilySymbol
        {
            get => lstFamilySymbol;
            set
            {
                lstFamilySymbol = value;
                OnPropertyChanged(nameof(LstFamilySymbol));
            }
        }
        public ExternalEvent ExternalEvent { get; set; }
        public CreatNewInstanceEEH Handler { get; set; }
        public List<Curve> Curves { get; set; }
        public List<XYZ> Points { get; set; }

        public bool IsSave { get; set; }
        public ICommand CreatCommand { get; set; }
        public ICommand PickPointCommand { get; set; }
        public ICommand PickLineCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CreatNewInstanceVM()
        {
            IsSave = false;
            lstFamilySymbol = new ObservableCollection<Object>();
            Curves = new List<Curve>();
            Points = new List<XYZ>();
            CreatCommand = new RelayCommand(Creat, CanCreat);
            PickPointCommand = new RelayCommand(PickPoint, CanPick);
            PickLineCommand = new RelayCommand(PickLine, CanPick);
        }

        private void PickPoint(object obj)
        {
            Handler.Mode = 1;
            Handler.SelectedCategory = SelectedCategory;
            Handler.SelectedFamilySymbol = SelectedFamilySymbol;
            Handler.SelectedLevel = SelectedLevel;
            Handler.Points = Points;
            Handler.Curves = Curves;
            ExternalEvent.Raise();
        }

        private bool CanPick(object obj)
        {
            if (SelectedLevel == null || SelectedCategory == null || SelectedFamilySymbol == null) return false;
            return true;
        }

        private void PickLine(object obj)
        {
            Handler.Mode = 2;
            Handler.SelectedCategory = SelectedCategory;
            Handler.SelectedFamilySymbol = SelectedFamilySymbol;
            Handler.SelectedLevel = SelectedLevel;
            Handler.Points = Points;
            Handler.Curves = Curves;
            ExternalEvent.Raise();
        }

        private void LoadFamilySymbol(Category category)
        {
            if (category == null)
            {
                LstFamilySymbol.Clear();
                return;
            }
            FilteredElementCollector colector = new FilteredElementCollector(doc);
            if (category.BuiltInCategory == BuiltInCategory.OST_Walls)
            {
                LstFamilySymbol = new ObservableCollection<object>(
                                colector.OfCategory(category.BuiltInCategory)
                                .OfClass(typeof(WallType))
                                .WhereElementIsElementType()
                                .Cast<object>()
                                .ToList());
            }
            else if (category.BuiltInCategory == BuiltInCategory.OST_Roofs)
            {
                LstFamilySymbol = new ObservableCollection<object>(
                                colector.OfCategory(category.BuiltInCategory)
                                .OfClass(typeof(RoofType))
                                .WhereElementIsElementType()
                                .Cast<object>()
                                .ToList());
            }
            else if (category.BuiltInCategory == BuiltInCategory.OST_Floors)
            {
                LstFamilySymbol = new ObservableCollection<object>(
                                colector.OfCategory(category.BuiltInCategory)
                                .OfClass(typeof(FloorType))
                                .WhereElementIsElementType()
                                .Cast<object>()
                                .ToList());
            }
            else
            {
                LstFamilySymbol = new ObservableCollection<object>(
                                colector.OfCategory(category.BuiltInCategory)
                                .OfClass(typeof(FamilySymbol))
                                .WhereElementIsElementType()
                                .Cast<object>()
                                .ToList());
            }
        }

        private bool CanCreat(object obj)
        {
            if (Points.Count() <= 0 && Curves.Count() <= 0) return false;
            return true;
        }

        private void Creat(object obj)
        {
            Handler.Mode = 3;
            Handler.SelectedCategory = SelectedCategory;
            Handler.SelectedFamilySymbol = SelectedFamilySymbol;
            Handler.SelectedLevel = SelectedLevel;
            Handler.Points = Points;
            Handler.Curves = Curves;
            ExternalEvent.Raise();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}