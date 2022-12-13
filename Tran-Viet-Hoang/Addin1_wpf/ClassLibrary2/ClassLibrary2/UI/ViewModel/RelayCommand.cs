using System;
using System.Windows.Input;

namespace ClassLibrary2.UI.ViewModel
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action _a;

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