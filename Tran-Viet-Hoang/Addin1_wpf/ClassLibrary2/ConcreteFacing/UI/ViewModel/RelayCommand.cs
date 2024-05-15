using System;
using System.Windows.Input;

namespace ConcreteFacing.UI.ViewModel
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

    public class RelayCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<T> _a;

        public RelayCommand(Action<T> a)
        {
            _a = a;
        }

        public bool CanExecute(object parameter)
        {
            return parameter.GetType().IsClass
                   && parameter != null
                   && parameter is T
                   && _a != null;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _a.Invoke((T)parameter);
        }
    }
}