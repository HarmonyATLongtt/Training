using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for NhanVienView.xaml
    /// </summary>
    public partial class NhanVienView : Window
    {
        public NhanVienView()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 || e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3 || e.Key == Key.D4 || e.Key == Key.D5 ||
                e.Key == Key.D6 || e.Key == Key.D7 || e.Key == Key.D8 || e.Key == Key.D9 || e.Key == Key.OemPeriod || e.Key == Key.Back)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (System.Windows.Controls.TextBox)sender;
            string fullText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !IsValidDouble(fullText);
        }

        private bool IsValidDouble(string text)
        {
            if (text[0] == '.')
            {
                return false;
            }
            try
            {
                double result = Convert.ToDouble(text);
                if (result > 100000000)
                {
                    return false;
                }
            }
            catch
            {
            }

            string[] parts = text.Split('.');
            if (parts.Length > 2)
            {
                return false;
            }
            return true;
        }
    }
}