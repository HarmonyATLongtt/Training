using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using TreeViewProject.Model;

namespace TreeViewProject.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public TreeViewModel TreeVM { get; }

        private IClipboardHandler? _activeHandler;

        public IClipboardHandler? ActiveHandler
        {
            get => _activeHandler;
            set { _activeHandler = value; OnPropertyChanged(); }
        }

        private NodeViewModel? _selectedNode;

        public NodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set { _selectedNode = value; OnPropertyChanged(); }
        }

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
            CopyCommand = new RelayCommand(_ => ActiveHandler?.CopyCommand.Execute(null));
            CutCommand = new RelayCommand(_ => ActiveHandler?.CutCommand.Execute(null));
            PasteCommand = new RelayCommand(_ => ActiveHandler?.PasteCommand.Execute(null));
            DeleteCommand = new RelayCommand(_ => ActiveHandler?.DeleteCommand.Execute(null));
            HelpCommand = new RelayCommand(_ => ActiveHandler?.HelpCommand.Execute(null));
        }
    }

    public interface IClipboardHandler
    {
        ICommand CopyCommand { get; }
        ICommand CutCommand { get; }
        ICommand PasteCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand HelpCommand { get; set; }
    }
}