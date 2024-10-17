using System.Windows;
using System.Windows.Input;

namespace WPF_Ex
{
    public partial class View : Window
    {
        public View()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(); 
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) 
            {
                Close(); 
            }
         
            else if (e.Key == Key.F5)
            {
               
            }
        }

        
    }
}
