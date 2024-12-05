using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPF_Ex.Model;

namespace WPF_Ex.ViewModels
{
    public class ItemViewModel : BindableBase
    {
        private ItemModel _model { get; set; }
        public string SheetName
        {
            get => _model.SheetName;
            set
            {
                _model.SheetName = value;
                RaisePropertyChanged(nameof(_model.SheetName));
            }
        }

        public ObservableCollection<Person> People
        {
            get => _model.People;
            set
            {
                _model.People = value;
                RaisePropertyChanged(nameof(_model.People));
            }
        }

        public ItemViewModel(ItemModel model)
        {
            _model = model;
        }
    }
}
