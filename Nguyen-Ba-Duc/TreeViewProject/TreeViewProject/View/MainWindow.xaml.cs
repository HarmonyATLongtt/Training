using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TreeViewProject.ViewModel;

namespace TreeViewProject.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainVM;

        public MainWindow()
        {
            InitializeComponent();
            _mainVM = new MainViewModel();
            this.DataContext = _mainVM;
        }

        //
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_mainVM == null) return;

            // Ctrl + C
            if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_mainVM.CopyCommand?.CanExecute(null) == true)
                    _mainVM.CopyCommand.Execute(null);
                e.Handled = true; // Nếu muốn chặn TextBox copy mặc định
            }

            // Ctrl + V
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_mainVM.PasteCommand?.CanExecute(null) == true)
                    _mainVM.PasteCommand.Execute(null);
                e.Handled = true; // Nếu muốn chặn TextBox paste mặc định
            }
            // Ctrl + X
            if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_mainVM.CutCommand?.CanExecute(null) == true)
                    _mainVM.CutCommand.Execute(null);
                e.Handled = true; // Nếu muốn chặn TextBox cut mặc định
            }

            // Delete
            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (_mainVM.DeleteCommand?.CanExecute(null) == true)
                    _mainVM.DeleteCommand.Execute(null);
                e.Handled = true; // thường Delete bạn muốn override hoàn toàn
            }

            // F1
            if (e.Key == Key.F1 && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (_mainVM.HelpCommand?.CanExecute(null) == true)
                    _mainVM.HelpCommand.Execute(null);
                e.Handled = true; // chặn F1 mặc định của Windows
            }
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

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is NodeViewModel node)
            {
                _mainVM.TreeVM.SelectedNode = node;
                //_mainVM.SelectedNode = node;
            }
        }
    }
}