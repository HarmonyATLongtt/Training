using CreateFamily.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateFamily.ViewModel
{
    class ItemCheckViewModel : INotifyPropertyChanged
    {
        public ItemCheckModel _model;
        public bool IsChecked
        {
            get { return _model.IsChecked; }
            set
            {
                _model.IsChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public string Name
        {
            get { return _model.Name; }
            set
            {
                _model.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public ItemCheckViewModel(ItemCheckModel model)
        {
            _model = model;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
