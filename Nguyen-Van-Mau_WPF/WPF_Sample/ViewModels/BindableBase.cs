using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF_Sample.ViewModels
{
    /// <summary>
    /// base class for all view models that
    /// implements INotifyPropertyChanged interface
    /// </summary>
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        public virtual void OnPropertyChanged(string? propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SetProperty<T>(ref T member, T val, [CallerMemberName] string? propertyName = null)
        {
            if (val != null && member != null && val.Equals(member))
            {
                return;
            }

            member = val;
            OnPropertyChanged(propertyName);
        }
    }
}