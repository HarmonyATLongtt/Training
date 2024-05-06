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

namespace Combobox_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Food> listname;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //listname = new List<string> { "nguyen van A", "nguyen van B", "nguyen van C" };
            cb2.ItemsSource = listname;
            listname = new List<Food>() {
            new Food(){Name = "san pham 1", Price = "200.000"},
            new Food(){Name = "san pham 2", Price = "50.000"},
            new Food(){Name = "san pham 3", Price = "75.000"}
            };
            cb2.ItemsSource = listname;
            cb2.DisplayMemberPath = "Name";
            cb2.SelectedValuePath = "Price";

            cbColor.ItemsSource = typeof(Brushes).GetProperties();

            cb2.SelectionChanged += cbItemSource_SelectionChanged;

            cb2.SelectionChanged += cbItemSource_SelectionChanged;
        }

        private void cbItemSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show(cb2.SelectedValue.ToString());
        }
    }

    public class Food
    {
        public string Name { get; set; }
        public string Price { get; set; }
    }
}