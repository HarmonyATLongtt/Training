using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SolutionRevitAPI.WPF.ViewModels
{
    public class EditDistanceGridsVM : INotifyPropertyChanged
    {
        private double distance;
        private string note;

        public double Distance
        {
            get => distance;
            set
            {
                distance = value;
                OnPropertyChanged(nameof(Distance));
            }
        }

        public bool IsSave { get; set; }
        public ICommand SaveCommand { get; set; }
        public string Note { get => note; set => note = value; }

        public EditDistanceGridsVM()
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}