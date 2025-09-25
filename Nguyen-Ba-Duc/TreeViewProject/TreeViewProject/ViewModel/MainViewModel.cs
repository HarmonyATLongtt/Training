using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TreeViewProject.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public TreeViewModel TreeVM { get; set; }

        private object? _activeHandler;

        public object? ActiveHandler
        {
            get => _activeHandler;
            set { _activeHandler = value; OnPropertyChanged(); }
        }

        private object? _clipBoardObj;

        public object? ClipBoardObj
        {
            get => _clipBoardObj;
            set
            {
                if (_clipBoardObj is NodeViewModel && value is string)
                {
                    TreeVM.UnMarkedAll();
                }
                _clipBoardObj = value; OnPropertyChanged(nameof(ClipBoardObj));

                if (ClipBoardObj != null)
                {
                    if (ClipBoardObj is string name) { ClipBoardName = name; }
                    if (ClipBoardObj is NodeViewModel node) { ClipBoardName = node.Name; }
                }
            }
        }

        //test

        private string _clipBoardName;

        public string ClipBoardName
        {
            get => _clipBoardName;
            set { _clipBoardName = value; OnPropertyChanged(nameof(ClipBoardName)); }
        }

        //private NodeViewModel? _selectedNode;

        //public NodeViewModel? SelectedNode
        //{
        //    get => _selectedNode;
        //    set { _selectedNode = value; OnPropertyChanged(); }
        //}

        //public TextBox? ActiveTextBox { get; set; }

        public ICommand CopyCommand { get; set; }
        public ICommand CutCommand { get; set; }
        public ICommand PasteCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand HelpCommand { get; set; }

        public MainViewModel()
        {
            TreeVM = new TreeViewModel();
            ActiveHandler = TreeVM;

            InvokeCommand();
        }

        private void InvokeCommand()
        {
            CopyCommand = new RelayCommand(_ => OnCopy());
            PasteCommand = new RelayCommand(_ => OnPaste());
            CutCommand = new RelayCommand(_ => OnCut());
            DeleteCommand = new RelayCommand(_ => OnDelete());
            HelpCommand = new RelayCommand(_ => OnHelp());

            //CopyCommand = new RelayCommand(_ => ActiveHandler?.CopyCommand.Execute(null));
            //CutCommand = new RelayCommand(_ => ActiveHandler?.CutCommand.Execute(null));
            //PasteCommand = new RelayCommand(_ => ActiveHandler?.PasteCommand.Execute(null));
            //DeleteCommand = new RelayCommand(_ => ActiveHandler?.DeleteCommand.Execute(null));
            //HelpCommand = new RelayCommand(_ => ActiveHandler?.HelpCommand.Execute(null));
        }

        private void OnCopy()
        {
            (ActiveHandler as ICopyPasteHandler)?.Copy(this);
        }

        private void OnPaste()
        {
            if (ClipBoardObj is NodeViewModel)
            {
                (ActiveHandler as INodeEditHandler)?.Paste(this);
            }
            else if (ClipBoardObj is string)
            {
                (ActiveHandler as ICopyPasteHandler)?.Paste(this);
            }

            //else if (ClipBoardObj is string text && ActiveTextBox != null)
            //{
            //    ActiveTextBox.SelectedText = text; // paste vào TextBox đang focus
            //}
        }

        private void OnCut()
        {
            (ActiveHandler as INodeEditHandler)?.Cut(this);
        }

        private void OnDelete()
        {
            (ActiveHandler as INodeEditHandler)?.Delete();
        }

        private void OnHelp()
        {
            if (ActiveHandler is INodeEditHandler handler)
            {
                handler.ShowHelp();
            }
            else if (TreeVM.CurrentHoveredNode != null)
            {
                TreeVM.ShowHelp();
            }
        }
    }

    //public interface IClipboardHandler
    //{
    //    ICommand CopyCommand { get; }
    //    ICommand CutCommand { get; }
    //    ICommand PasteCommand { get; }
    //    ICommand DeleteCommand { get; }
    //    ICommand HelpCommand { get; set; }
    //}
    public interface ICopyPasteHandler
    {
        void Copy(MainViewModel mainVM);

        void Paste(MainViewModel mainVM);
    }

    public interface INodeEditHandler : ICopyPasteHandler
    {
        void Cut(MainViewModel mainVM);

        void Delete();

        void ShowHelp();
    }
}