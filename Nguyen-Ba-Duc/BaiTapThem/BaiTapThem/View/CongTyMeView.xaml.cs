using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BaiTapThem.ViewModel;

namespace BaiTapThem.View
{
    /// <summary>
    /// Interaction logic for CongTyMeView.xaml
    /// </summary>
    public partial class CongTyMeView : Window
    {
        public CongTyMeView()
        {
            InitializeComponent();
            DataContext = new CongTyMeViewModel(this);
            KeyDown += Window_KeyDown;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ImportView importView = new ImportView();
            this.Close();
            importView.Show();
        }
    }
}