using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace BaiTapThem.ViewModel
{
    public class BaseViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    //internal class RelayCommand<T> : ICommand
    //{
    //    private readonly Predicate<T> _canExecute;
    //    private readonly Action<T> _execute;

    //    public RelayCommand(Predicate<T> canExecute, Action<T> execute)
    //    {
    //        if (execute == null)
    //            throw new ArgumentNullException("execute");
    //        _canExecute = canExecute;
    //        _execute = execute;
    //    }

    //    public bool CanExecute(object parameter)
    //    {
    //        return _canExecute == null ? true : _canExecute((T)parameter);
    //    }

    //    public void Execute(object parameter)
    //    {
    //        _execute((T)parameter);
    //    }

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add { CommandManager.RequerySuggested += value; }
    //        remove { CommandManager.RequerySuggested -= value; }
    //    }
    //}

    // Cach su dung command

    //public RelayCommand ConfirmCommand { get; }
    //public MyDialogViewModel()
    //{
    //    ConfirmCommand = new RelayCommand(ExecuteConfirmCommand, CanExecuteConfirmCommand);
    //    CancelCommand = new RelayCommand(ExecuteCancelCommand);
    //}

    //private void ExecuteConfirmCommand(object parameter)
    //{
    //    MessageBox.Show($"Confirmed with input: {InputText}");
    //    if (parameter is Window window)
    //    {
    //        window.DialogResult = true;
    //        window.Close();
    //    }
    //}

    //private bool CanExecuteConfirmCommand(object parameter)
    //{
    //    // Chỉ cho phép xác nhận khi InputText không rỗng
    //    return !string.IsNullOrWhiteSpace(InputText);
    //}

    //private void ExecuteCancelCommand(object parameter)
    //{
    //    if (parameter is Window window)
    //    {
    //        window.DialogResult = false;
    //        window.Close();
    //    }
    //}
}