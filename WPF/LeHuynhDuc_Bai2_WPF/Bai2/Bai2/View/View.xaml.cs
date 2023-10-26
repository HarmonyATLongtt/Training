using Bai2.ViewModel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using Moq;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Bai2.View
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class View : UserControl
    {
        public View()
        {
            InitializeComponent();
            DataContext = new ViewModelMain();
        }

      
    }
}
