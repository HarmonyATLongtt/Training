using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WPF_Ex.Model;
using WPF_Ex.ViewModels;

namespace WPF_Ex
{
    public partial class View : Window
    {
        public View()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();

        }
    }
}
