using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel;

namespace SolutionRevitAPI.WPF.Model
{
    public class CategoryTag : INotifyPropertyChanged
    {
        private bool isSelected;
        private Category cat;
        private List<FamilySymbol> loadedTags;
        private FamilySymbol selectedMember;

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
        public List<FamilySymbol> LoadedTags { get => loadedTags; set => loadedTags = value; }

        public FamilySymbol SelectedMember
        {
            get => selectedMember;
            set
            {
                selectedMember = value;
                OnPropertyChanged(nameof(SelectedMember));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}