using System;
using System.Windows;
using System.Windows.Input;

namespace ClassLibrary2.UI.ViewModel
{

    public class ViewClose
    {
        public ICommand CloseCommand { get => new HelperCommand(UserClose, CanClose); }

        private bool CanClose(object parameter) => true;
        private void UserClose(object parameter)
        {
            Window wnd = parameter as Window;
            MessageBox.Show("Đây là view mới");
            wnd?.Close();
        }

       
    }
    public class HelperCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        //public HelperCommand(Action<object> execute) : this(execute, canExecute: null) { }
        public HelperCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            this._execute = execute;
            this._canExecute = canExecute;
        }
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => this._canExecute == null ? true : this._canExecute(parameter);
        public void Execute(object parameter) => this._execute(parameter);
        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
