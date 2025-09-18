using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewProject.ViewModel
{
    using System.Windows.Controls;
    using System.Windows.Input;

    public class TextEditorViewModel : BaseViewModel, IClipboardHandler
    {
        private readonly TextBox _textBox;

        public ICommand CopyCommand { get; set; }
        public ICommand CutCommand { get; set; }
        public ICommand PasteCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand HelpCommand { get; set; }

        public TextEditorViewModel(TextBox textBox)
        {
            _textBox = textBox;

            CopyCommand = new RelayCommand(_ => ApplicationCommands.Copy.Execute(null, _textBox));
            CutCommand = new RelayCommand(_ => ApplicationCommands.Cut.Execute(null, _textBox));
            PasteCommand = new RelayCommand(_ => ApplicationCommands.Paste.Execute(null, _textBox));
            DeleteCommand = new RelayCommand(_ =>
            {
                if (!string.IsNullOrEmpty(_textBox.SelectedText))
                    _textBox.SelectedText = "";
            });
        }
    }
}