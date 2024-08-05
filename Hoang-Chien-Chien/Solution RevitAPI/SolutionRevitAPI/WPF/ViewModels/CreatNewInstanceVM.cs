using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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

        public bool IsSave { get; set; }
        public ICommand CreatCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CreatNewInstanceVM()
        {
            IsSave = false;
            lstFamilySymbol = new ObservableCollection<Object>();
            CreatCommand = new RelayCommand(Creat, CanCreat);
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
            } else if (category.BuiltInCategory == BuiltInCategory.OST_Roofs)
            {
                LstFamilySymbol = new ObservableCollection<object>(
                                colector.OfCategory(category.BuiltInCategory)
                                .OfClass(typeof(RoofType))
                                .WhereElementIsElementType()
                                .Cast<object>()
                                .ToList());
            } else if (category.BuiltInCategory == BuiltInCategory.OST_Floors)
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
            if (SelectedLevel == null || SelectedCategory == null || SelectedFamilySymbol == null) return false;
            return true;
        }

        private void Creat(object obj)
        {
            Window window = obj as Window;
            var result = MessageBox.Show("Bạn có chắc muốn thực hiện những thay đổi này không?", "Warning", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                return;
            IsSave = true;
            window.Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}