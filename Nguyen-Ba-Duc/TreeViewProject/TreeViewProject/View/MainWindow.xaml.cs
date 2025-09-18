using System.Diagnostics;
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
        private MainViewModel _mainVM;
        //private TextEditorViewModel _textEditorVM;

        public MainWindow()
        {
            InitializeComponent();
            _mainVM = new MainViewModel();
            this.DataContext = _mainVM;

            //_textEditorVM = new TextEditorViewModel(TextEditor);
        }

        private void TreeView_GotFocus(object sender, RoutedEventArgs e)
        {
            _mainVM.ActiveHandler = _mainVM.TreeVM;
        }

        private void TreeView_LostFocus(object sender, RoutedEventArgs e)
        {
            //Nếu toàn bộ TreeView vẫn chứa focus ở bên trong, thì bỏ qua
            if (MyTree.IsKeyboardFocusWithin)
                return;

            //Lấy control hiện tại đang focus
            var focusedElement = Keyboard.FocusedElement as FrameworkElement;

            if (focusedElement != null)
            {
                string name = focusedElement.Name;

                if (name == "CopyButton" || name == "CutButton" || name == "PasteButton" || name == "DeleteButton")
                {
                    return;
                }
            }

            // Thật sự mất focus ra ngoài
            _mainVM.ActiveHandler = null;
        }

        //private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    _mainVM.ActiveHandler = _textEditorVM;
        //}

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NodeViewModel node)
            {
                _mainVM.TreeVM.SelectedNode = node;
                _mainVM.SelectedNode = node;
            }
        }
    }
}