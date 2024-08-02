using Autodesk.Revit.DB;
using System.ComponentModel;

namespace SolutionRevitAPI.WPF.Model
{
    public class CreatFilterForView_M : INotifyPropertyChanged
    {
        private bool isSelected;
        private Category cat;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public Category Cat { get => cat; set => cat = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}