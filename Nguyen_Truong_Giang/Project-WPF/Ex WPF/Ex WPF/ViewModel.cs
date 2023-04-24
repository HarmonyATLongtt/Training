using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ex_WPF.MainWindow;

namespace Ex_WPF
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertiesChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private Model _model;

        public ObservableCollection<Student> Students
        {
            get => _model.Students;
            set
            {
                _model.Students = value;
                RaisePropertiesChanged(nameof(Students));
            }
        }

        public string Test
        {
            get => _model.Test;
            set
            {
                _model.Test = value;

                RaisePropertiesChanged(nameof(Test));
            }
        }



        public ViewModel(Model model)
        {
            _model = model;
        }
    }
}
