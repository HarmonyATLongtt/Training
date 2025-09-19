using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class NodeViewModel : BaseViewModel
    {
        //private readonly MainViewModel _mainVM;
        private readonly TreeViewModel _treeVM;

        private readonly NodeModel _node;
        public string? Name => _node.Name;

        public object? ItemInfo => _node.ItemInfo;

        public ObservableCollection<NodeViewModel> Children { get; }

        public bool IsExpanded
        {
            get => _node.IsExpanded;
            set
            {
                _node.IsExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public bool IsMarked
        {
            get => _node.IsMarked;
            set
            {
                _node.IsMarked = value;
                OnPropertyChanged(nameof(IsMarked));
            }
        }

        public string? ImagePath
        {
            get => _node.ImagePath;
            set
            {
                _node.ImagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public string? GifPath
        {
            get => _node.GifPath;
            set
            {
                _node.GifPath = value;
                OnPropertyChanged(nameof(GifPath));
            }
        }

        public string? VideoPath
        {
            get => _node.VideoPath;
            set
            {
                _node.VideoPath = value;
                OnPropertyChanged(nameof(VideoPath));
            }
        }

        public string? Description
        {
            get => _node.Description;
            set
            {
                _node.Description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public string? DetailDescription
        {
            get => _node.DetailDescription;
            set
            {
                _node.DetailDescription = value;
                OnPropertyChanged(nameof(DetailDescription));
            }
        }

        public NodeViewModel? Parent { get; set; }

        private DispatcherTimer _tooltipTimer;

        private Visibility _isDetailVisible = Visibility.Collapsed;

        public Visibility IsDetailVisible
        {
            get => _isDetailVisible;

            set
            {
                _isDetailVisible = value; OnPropertyChanged(nameof(IsDetailVisible));
            }
        }

        public ICommand TooltipOpenedCommand { get; set; }
        public ICommand TooltipClosedCommand { get; set; }

        public NodeViewModel(NodeModel node, TreeViewModel treeVM, NodeViewModel? parent = null, bool buildChildren = true)
        {
            _node = node;
            Parent = parent;
            _treeVM = treeVM;
            Children = new ObservableCollection<NodeViewModel>();

            if (buildChildren)
            {
                foreach (var child in _node.Children)
                {
                    Children.Add(new NodeViewModel(child, treeVM, this));
                }
            }
            InvokeCommand();
        }

        private void InvokeCommand()
        {
            TooltipOpenedCommand = new RelayCommand(_ => TooltipOpenedCommandInvoke());
            TooltipClosedCommand = new RelayCommand(_ => TooltipClosedCommandInvoke());
        }

        private void TooltipOpenedCommandInvoke()
        {
            _tooltipTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _tooltipTimer.Tick += (s, e) =>
            {
                _tooltipTimer.Stop();
                IsDetailVisible = Visibility.Visible;
                _treeVM.CurrentHoveredNode = this;
            };
            _tooltipTimer.Start();
        }

        private void TooltipClosedCommandInvoke()
        {
            _tooltipTimer?.Stop();
            if (_treeVM.CurrentHoveredNode == this)
                _treeVM.CurrentHoveredNode = null;
            IsDetailVisible = Visibility.Collapsed;
            CommandManager.InvalidateRequerySuggested();
        }

        public NodeViewModel Clone(TreeViewModel treeVM, NodeViewModel? parent = null)
        {
            var clone = new NodeViewModel(_node, treeVM, parent, buildChildren: false)
            {
                Description = this.Description,
                DetailDescription = this.DetailDescription,
                ImagePath = this.ImagePath,
                VideoPath = this.VideoPath,
                GifPath = this.GifPath
            };

            foreach (var child in Children)
            {
                var clonedChild = child.Clone(treeVM, clone);
                clone.Children.Add(clonedChild);
            }

            return clone;
        }
    }
}