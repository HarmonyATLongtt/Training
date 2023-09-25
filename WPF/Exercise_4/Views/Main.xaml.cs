using System.Windows;

namespace Exercise_4.Views
{
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }
    }
}