using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TreeViewProject.Model;
using TreeViewProject.ViewModel;

namespace TreeViewProject.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NodeViewModel node)
            {
                _viewModel.SelectedNode = node;
            }
        }

        //private void PlayVideo_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button btn)
        //    {
        //        VideoPlayer?.Play();
        //    }
        //}
    }
}