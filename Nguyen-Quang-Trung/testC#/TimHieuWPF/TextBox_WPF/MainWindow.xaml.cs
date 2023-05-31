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

namespace TexBox_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            outputValue.Text = inputValue.Text;
        }

        private int DoubleValue(int value)
        {
            return value * 2;
        }

        private void InputValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value = 0;
            if (int.TryParse(inputValue.Text, out value))
            {
                outputValue.Text = DoubleValue(value).ToString();
            }
            else
            {
                outputValue.Text = "ban nhap sai vui long nhap lai";
            }
        }
    }
}