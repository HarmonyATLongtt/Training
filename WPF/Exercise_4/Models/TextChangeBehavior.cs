using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Exercise_1.Models
{
    public class TextChangeBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            TextBox textBox = AssociatedObject as TextBox;
            textBox.TextChanged += TextBox_TextChanged;
            if(string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                textBox.Background = new SolidColorBrush(Colors.Red);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = AssociatedObject as TextBox;
            textBox.TextChanged += TextBox_TextChanged;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Background = new SolidColorBrush(Colors.Green);
            }
            else
                textBox.Background = new SolidColorBrush(Colors.Red);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            TextBox textBox = AssociatedObject as TextBox;
            textBox.TextChanged -= TextBox_TextChanged;
        }
    }
}
