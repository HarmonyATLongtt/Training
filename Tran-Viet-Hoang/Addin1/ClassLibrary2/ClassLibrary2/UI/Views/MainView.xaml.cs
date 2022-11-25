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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace ClassLibrary2.UI.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void btnload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Access files (*.mdb)|*.mdb";
            if (openFileDialog.ShowDialog() == true)
                txtinput.Text = openFileDialog.SafeFileName;
            //txtinput.Text = File.ReadAllText(openFileDialog.FileName);
        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
        }

        private void dgrsheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
