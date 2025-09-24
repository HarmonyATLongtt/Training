using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using TreeViewProject.ViewModel;

namespace TreeViewProject.Behaviors
{
    public class CopyPasteBehavior : Behavior<TextBox>
    {
        public MainViewModel MainVM
        {
            get => (MainViewModel)GetValue(MainVMProperty);
            set => SetValue(MainVMProperty, value);
        }

        public static readonly DependencyProperty MainVMProperty =
            DependencyProperty.Register(nameof(MainVM), typeof(MainViewModel),
                typeof(CopyPasteBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (MainVM != null)
            {
                // TextBoxVM sẽ đóng vai trò ICopyPasteHandler
                MainVM.ActiveHandler = new TextBoxCopyPasteHandler(
                    AssociatedObject, AssociatedObject.IsReadOnly);
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (MainVM != null && MainVM.ActiveHandler is TextBoxCopyPasteHandler handler
                && handler.TextBox == AssociatedObject)
            {
                // Lấy control hiện đang nhận focus
                var focusedElement = Keyboard.FocusedElement as DependencyObject;

                // Nếu focus mới là Button Copy/Paste thì KHÔNG reset handler
                if (focusedElement is Button button &&
                    (button.Name == "CopyButton" || button.Name == "PasteButton"))
                {
                    return; // giữ nguyên ActiveHandler
                }

                MainVM.ActiveHandler = null;
            }
        }
    }

    /// <summary>
    /// Triển khai ICopyPasteHandler cho TextBox
    /// </summary>
    public class TextBoxCopyPasteHandler : ICopyPasteHandler
    {
        public TextBox TextBox { get; }
        private readonly bool _isReadOnly;

        public TextBoxCopyPasteHandler(TextBox textBox, bool isReadOnly)
        {
            TextBox = textBox;
            _isReadOnly = isReadOnly;
        }

        public void Copy(MainViewModel mainVM)
        {
            if (!string.IsNullOrEmpty(TextBox.SelectedText))
            {
                mainVM.ClipBoardObj = TextBox.SelectedText;
            }
        }

        public void Paste(MainViewModel mainVM)
        {
            if (_isReadOnly) return; // chỉ copy được, không paste

            if (mainVM.ClipBoardObj is string text)
            {
                TextBox.SelectedText = text;
                //var caret = TextBox.CaretIndex;
                //TextBox.Text = TextBox.Text.Insert(caret, text);
                //TextBox.CaretIndex = caret + text.Length;
            }
        }
    }
}