using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace WPF_Ex.Behaviors
{
    public class WindowBehavior : Behavior<Window>
    {
        // Command được bind cho phím Escape
        public ICommand EscapeCommand
        {
            get { return (ICommand)GetValue(EscapeCommandProperty); }
            set { SetValue(EscapeCommandProperty, value); }
        }
        public static readonly DependencyProperty EscapeCommandProperty =
            DependencyProperty.Register("EscapeCommand", typeof(ICommand), typeof(WindowBehavior), new PropertyMetadata(null));

        // Command được bind cho phím F5
        public ICommand F5Command
        {
            get { return (ICommand)GetValue(F5CommandProperty); }
            set { SetValue(F5CommandProperty, value); }
        }
        public static readonly DependencyProperty F5CommandProperty =
            DependencyProperty.Register("F5Command", typeof(ICommand), typeof(WindowBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
            base.OnDetaching();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Truyền cửa sổ (AssociatedObject) làm parameter
                if (EscapeCommand != null && EscapeCommand.CanExecute(AssociatedObject))
                {
                    EscapeCommand.Execute(AssociatedObject);
                }
                else
                {
                    AssociatedObject.Close();
                }
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                if (F5Command != null && F5Command.CanExecute(AssociatedObject))
                {
                    F5Command.Execute(AssociatedObject);
                    e.Handled = true;
                }
            }
        }

    }
}
