using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ConcreteFacing.DATA
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public bool _cateSelected { get; set; }
        public bool CateIsChecked
        {
            get => _cateSelected;
            set
            {
                if (_cateSelected != value)
                {
                    _cateSelected = value;
                    OnPropertyChanged(nameof(CateIsChecked));
                }
            }
          
        }
        public List<Element> CateElems { get; set; }
        public string CateName { get; set; }

        public ICommand CateCmd { get; set; }

        public double Thickness { get; set; }

        public ObservableCollection<OptionViewModel> _templateCoverFaceViewModels { get; set; }

        public ObservableCollection<OptionViewModel> TemplateCoverFaceViewModels

        {
            get => _templateCoverFaceViewModels;
            set
            {
                if (_templateCoverFaceViewModels != value)
                {
                    _templateCoverFaceViewModels = value;
                    OnPropertyChanged(nameof(TemplateCoverFaceViewModels));
                }
            }
        }

        public CategoryViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}