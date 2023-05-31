using System;
using System.Windows.Input;

namespace Bai_2
{
    public class RelayCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };

        private Action<T> _execute;

        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke((T)parameter);
        }
    }
}