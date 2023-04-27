using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Bai_2.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // kích hoạt sự kiện thông báo khi thuộc tính thay đổi
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}