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

namespace Canvas_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btn1.Click += Btn1_Click;
            btn2.Click += Btn2_Click;
            btn3.Click += Btn3_Click;
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(btn1, 10);
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(btn2, 10);
        }

        private void Btn3_Click(object sender, RoutedEventArgs e)
        {
            Panel.SetZIndex(btn3, 10);
        }
    }
}