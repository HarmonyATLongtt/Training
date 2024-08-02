using Autodesk.Revit.DB;
using SolutionRevitAPI.WPF.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    public class TagAllVM : INotifyPropertyChanged
    {
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

        private FamilySymbol selectedMember;

        public FamilySymbol SelectedMember
        {
            get => selectedMember;
            set
            {
                selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
            }
        }

        private ObservableCollection<CategoryTag> lstCategoryTag;

        public ObservableCollection<CategoryTag> LstCategoryTag
        {
            get => lstCategoryTag;
            set
            {
                lstCategoryTag = value;
                OnPropertyChanged(nameof(LstCategoryTag));
            }
        }

        public bool IsSave { get; set; }

        public ICommand SaveCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TagAllVM()
        {
            IsSave = false;
            SaveCommand = new RelayCommand(Save, CanSave);
        }

        private bool CanSave(object obj)
        {
            return true;
        }

        private void Save(object obj)
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