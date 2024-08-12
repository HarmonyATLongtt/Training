using Autodesk.Revit.DB;
using System.ComponentModel;

namespace SolutionRevitAPI.WPF.Model
{
    public class EditFilterForView_M : INotifyPropertyChanged
    {
        private bool isSelected;
        private FilterElement fil;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public FilterElement Fil { get => fil; set => fil = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}