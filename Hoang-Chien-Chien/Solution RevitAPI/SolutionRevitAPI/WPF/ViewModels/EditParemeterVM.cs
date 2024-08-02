using SolutionRevitAPI.WPF.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    public class EditParemeterVM : INotifyPropertyChanged
    {
        private ObservableCollection<ParameterEle> parameterElements;

        public ObservableCollection<ParameterEle> ParameterElements
        {
            get => parameterElements;
            set
            {
                parameterElements = value;
                OnPropertyChanged(nameof(ParameterElements));
            }
        }

        public bool IsSave { get; set; }

        public ICommand SaveCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditParemeterVM()
        {
            ParameterElements = new ObservableCollection<ParameterEle>();
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