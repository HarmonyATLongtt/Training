using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClassLibrary2.UI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void RaisePropertyChange(string propName)
        {
            if (PropertyChanged!= null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        private string _myName;
        public string MyName 
        { 
            get { return _myName; } 
            set {
                _myName = value; 
                RaisePropertyChange(nameof(MyName));
            }
        }

        private int _myAge;
        public int MyAge
        {
            get { return _myAge; }
            set
            {
                _myAge = value;
                RaisePropertyChange(nameof(MyAge));
            }
        }

        public ICommand updateCommand { get; set; }

        public MainViewModel()
        {
            _myName = "Long";
            _myAge = 30;
            updateCommand = new RelayCommand(UpdateName);
        }

        private void UpdateName()
        {
            MyName = "Honag";
        }
    }

    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Action _a;

        public RelayCommand(Action a)
        {
            _a = a;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_a != null)
                _a.Invoke();
        }
    }
}
