using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TreeView_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MenuItem root = new MenuItem { Title = "Menu" };
            MenuItem child1 = new MenuItem { Title = "item 1" };
            child1.Items.Add(new MenuItem { Title = "child item 1.1" });
            child1.Items.Add(new MenuItem { Title = "child item 1.2" });
            MenuItem child2 = new MenuItem { Title = "item 2" }; ;
            root.Items.Add(child1);
            root.Items.Add(child2);
            trvMenu.Items.Add(root);
        }
    }

    public class MenuItem
    {
        public MenuItem()
        {
            this.Items = new ObservableCollection<MenuItem>();
        }

        public string Title { get; set; }

        public ObservableCollection<MenuItem> Items { get; set; }
    }
}