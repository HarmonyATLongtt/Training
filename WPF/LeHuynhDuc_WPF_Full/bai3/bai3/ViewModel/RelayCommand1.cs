using GalaSoft.MvvmLight.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace bai3.ViewModel
{
    public class RelayCommand1 : ICommand
    {
        #region Fields
        private readonly WeakAction _execute;

        private readonly WeakFunc<bool> _canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand1(Action execute, bool keepTargetAlive = false)
           : this(execute, null, keepTargetAlive)
        {
        }

        public RelayCommand1(Action execute, Func<bool> canExecute, bool keepTargetAlive = false)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = new WeakAction(execute, keepTargetAlive);
            if (canExecute != null)
            {
                _canExecute = new WeakFunc<bool>(canExecute, keepTargetAlive);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                if (_canExecute.IsStatic || _canExecute.IsAlive)
                {
                    return _canExecute.Execute();
                }

                return false;
            }

            return true;
        }

        public virtual void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null && (_execute.IsStatic || _execute.IsAlive))
            {
                _execute.Execute();
            }
        }
        #endregion
    }
}
