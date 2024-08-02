using Autodesk.Revit.DB;
using SolutionRevitAPI.WPF.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    internal class CreatFilterForView_VM : INotifyPropertyChanged
    {
        public Document Doc { get; set; }
        public ObservableCollection<PropertyInfo> ColorList { get; set; }
        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private ObservableCollection<CreatFilterForView_M> lstCategory;

        public ObservableCollection<CreatFilterForView_M> LstCategory
        {
            get => lstCategory;
            set
            {
                lstCategory = value;
                OnPropertyChanged(nameof(LstCategory));
            }
        }

        private string filterName;

        public string FilterName
        {
            get => filterName;
            set
            {
                filterName = value;
                OnPropertyChanged(nameof(FilterName));
            }
        }

        private bool isVisibleChecked;

        public bool IsVisibleChecked
        {
            get => isVisibleChecked;
            set
            {
                isVisibleChecked = value;
                OnPropertyChanged(nameof(IsVisibleChecked));
            }
        }

        private string colorLine;

        public string ColorLine
        {
            get => colorLine;
            set
            {
                colorLine = value;
                OnPropertyChanged(nameof(ColorLine));
            }
        }

        private int lineWeight;

        public int LineWeight
        {
            get => lineWeight;
            set
            {
                lineWeight = value;
                OnPropertyChanged(nameof(LineWeight));
            }
        }

        private string patternColor;

        public string PatternColor
        {
            get => patternColor;
            set
            {
                patternColor = value;
                OnPropertyChanged(nameof(PatternColor));
            }
        }

        private int transparency;

        public int Transparency
        {
            get => transparency;
            set
            {
                transparency = value;
                OnPropertyChanged(nameof(Transparency));
            }
        }

        private bool halftone;

        public bool Halftone
        {
            get => halftone;
            set
            {
                halftone = value;
                OnPropertyChanged(nameof(Halftone));
            }
        }

        public bool IsSave { get; set; }

        public ICommand SaveCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CreatFilterForView_VM()
        {
            ColorList = new ObservableCollection<PropertyInfo>(typeof(Brushes).GetProperties());
            IsVisibleChecked = true;
            IsSave = false;
            SaveCommand = new RelayCommand(Save, CanSave);
        }

        private bool CanSave(object obj)
        {
            bool checkSelectCat = false;
            foreach (var item in LstCategory)
            {
                if (!(item.IsSelected == true))
                {
                    continue;
                }
                checkSelectCat = true;
                break;
            }
            return !string.IsNullOrEmpty(FilterName) && checkSelectCat;
        }

        private void Save(object obj)
        {
            Window window = obj as Window;
            var result = MessageBox.Show("Bạn có chắc muốn thực hiện những thay đổi này không?", "Warning", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
                return;
            var checkFilterName = new FilteredElementCollector(Doc).OfClass(typeof(FilterElement)).ToElements().Cast<FilterElement>().ToList()
                                        .Where(p => p.Name == FilterName).FirstOrDefault();
            if (checkFilterName != null)
            {
                MessageBox.Show("Tên Filter này đã tồn tại, vui lòng chọn tên khác", "Warning");
                return;
            }
            IsSave = true;
            window.Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}