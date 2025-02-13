using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Ex.ViewModels
{
    public interface IEventArgsConverter
    {
        object Convert(object value, object parameter);
    }
}
