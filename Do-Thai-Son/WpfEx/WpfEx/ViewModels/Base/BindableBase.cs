using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfEx.ViewModels
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
        {
            if (member == null || !member.Equals(val))
            {
                member = val;
                RaisePropertyChanged(propertyName);
            }
        }
    }
}