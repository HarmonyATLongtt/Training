using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Bai2_WPF.Model;

namespace Bai2_WPF.ViewModel
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ItemModel Model { get; set; }    
        public string SheetName
        {
            get => Model.SheetName;
            set
            {
                Model.SheetName = value;
                OnPropertyChanged(nameof(SheetName));
            }
        }
        public ObservableCollection<Person> People
        {
            get => Model.People;
            set
            {
                Model.People = value;
                OnPropertyChanged(nameof(SheetName));
            }
        }
        public ItemViewModel(ItemModel model)
        {
            Model = model;
        }
    }
}
