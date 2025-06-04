using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstCommand.ViewModel;

namespace FirstCommand.Model
{
    public class ScheduleRowModel : BaseViewModel
    {
        private ObservableCollection<string> _values;

        public ObservableCollection<string> Values
        {
            get => _values;
            set
            {
                _values = value;
                OnPropertyChanged(nameof(Values));
            }
        }

        public ScheduleRowModel()
        {
            Values = new ObservableCollection<string>();
        }
    }
}