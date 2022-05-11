using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ex_RevitAPI.Bai_1.ViewModel
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canexecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Predicate<object> canexecute)
        {
            if (execute == null)
                throw new NullReferenceException("execute");
            _execute = execute;
            _canexecute = canexecute;
        }
        public RelayCommand(Action<object> execute) : this(execute, null)
        {

        }

        public bool CanExecute(object parameter)
        {
            return _canexecute == null ? true : _canexecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
