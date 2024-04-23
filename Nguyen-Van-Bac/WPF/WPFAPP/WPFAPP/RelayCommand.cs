using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace WPFAPP
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _callback;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> callback)
        {
            _callback = callback;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _callback?.Invoke(parameter);
        }
    }
}
