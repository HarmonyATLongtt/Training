using System.Collections.ObjectModel;

using System.Windows.Input;
using System.Windows.Media;
using TreeViewProject.Model;

namespace TreeViewProject.ViewModel
{
    public class TreeViewModel : BaseViewModel, INodeEditHandler
    {
        public ObservableCollection<NodeViewModel> RootNodes { get; set; }

        private NodeViewModel? _currentHoveredNode;

        public NodeViewModel? CurrentHoveredNode
        {
            get => _currentHoveredNode;
            set { _currentHoveredNode = value; OnPropertyChanged(nameof(CurrentHoveredNode)); }
        }

        private object? _currentViewModel;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
                if (CurrentViewModel != null)
                {
                    IsButtonCheckVisible = true;
                }
                else
                {
                    IsButtonCheckVisible = false;
                }
            }
        }

        private NodeViewModel? _selectedNode;

        public NodeViewModel? SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value; OnPropertyChanged(nameof(SelectedNode));

                if (SelectedNode?.ItemInfo is HumanViewModel)
                {
                    CurrentViewModel = (HumanViewModel)SelectedNode.ItemInfo;
                }
                else if (SelectedNode?.ItemInfo is ProductViewModel)
                {
                    CurrentViewModel = (ProductViewModel)SelectedNode.ItemInfo;
                }
                else if (SelectedNode?.ItemInfo == null)
                {
                    CurrentViewModel = null;
                }

                CheckStatusOfItemInfo();

                SelectedNodeImagePath = SelectedNode?.ImagePath;
                SelectedNodeGifPath = null;
                SelectedNodeVideoPath = null;
            }
        }

        private string _checkContent;

        public string CheckContent
        {
            get => _checkContent;
            set { _checkContent = value; OnPropertyChanged(nameof(CheckContent)); }
        }

        private bool _isButtonCheckVisible = false;

        public bool IsButtonCheckVisible
        {
            get => _isButtonCheckVisible;
            set { _isButtonCheckVisible = value; OnPropertyChanged(); }
        }

        private bool _isCheckControlVisible = false;

        public bool IsCheckControlVisible
        {
            get => _isCheckControlVisible;
            set { _isCheckControlVisible = value; OnPropertyChanged(nameof(IsCheckControlVisible)); }
        }

        private bool _isImageVisible;

        public bool IsImageVisible
        {
            get => _isImageVisible;
            set { _isImageVisible = value; OnPropertyChanged(); }
        }

        private bool _isGifVisible;

        public bool IsGifVisible
        {
            get => _isGifVisible;
            set { _isGifVisible = value; OnPropertyChanged(); }
        }

        private bool _isVideoVisible;

        public bool IsVideoVisible
        {
            get => _isVideoVisible;
            set { _isVideoVisible = value; OnPropertyChanged(); }
        }

        private string? _selectedNodeImagePath;

        public string? SelectedNodeImagePath
        {
            get { return _selectedNodeImagePath; }
            set
            {
                _selectedNodeImagePath = value; OnPropertyChanged(nameof(SelectedNodeImagePath));

                if (SelectedNodeImagePath != null)
                {
                    IsImageVisible = true;
                }
                else { IsImageVisible = false; }
            }
        }

        private string? _selectedNodeVideoPath;

        public string? SelectedNodeVideoPath
        {
            get { return _selectedNodeVideoPath; }
            set
            {
                _selectedNodeVideoPath = value; OnPropertyChanged(nameof(SelectedNodeVideoPath));
                if (SelectedNodeVideoPath != null)
                {
                    IsVideoVisible = true;
                }
                else { IsVideoVisible = false; }
            }
        }

        private string? _selectedNodeGifPath;

        public string? SelectedNodeGifPath
        {
            get => _selectedNodeGifPath;
            set
            {
                _selectedNodeGifPath = value; OnPropertyChanged(nameof(SelectedNodeGifPath));
                if (SelectedNodeGifPath != null)
                {
                    IsGifVisible = true;
                }
                else { IsGifVisible = false; }
            }
        }

        private NodeViewModel? _selectedCopyNode;

        public NodeViewModel? SelectedCopyNode
        {
            get { return _selectedCopyNode; }
            set
            {
                _selectedCopyNode = value; OnPropertyChanged(nameof(SelectedNode));
            }
        }

        //private NodeViewModel? _clipboardNode;

        //private NodeViewModel? _clipBoardNode;

        //public NodeViewModel? ClipBoardNode
        //{
        //    get => _clipBoardNode;
        //    set
        //    {
        //        if (_clipBoardNode != value)
        //        {
        //            _clipBoardNode = value;
        //            OnPropertyChanged(nameof(ClipBoardNode));
        //        }
        //    }
        //}

        private bool _isCut = false;

        public ICommand CheckCommand { get; set; }
        public ICommand HoverCommand { get; set; }
        public ICommand LeaveCommand { get; set; }
        public ICommand ExpandAllCommand { get; set; }
        public ICommand CollapseAllCommand { get; set; }
        public ICommand InvertCommand { get; set; }
        public ICommand CopyCommand { get; set; }
        public ICommand CutCommand { get; set; }
        public ICommand PasteCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        //public ICommand HelpCommand { get; set; }

        public TreeViewModel()
        {
            InitializeData();
            InvokeCommand();
        }

        private void InitializeData()
        {
            var human1 = new HumanModel
            {
                Name = "Nguyễn Bá Đức",
                Birthday = new DateTime(1994, 2, 5),
                Address = "Bắc Ninh",
                Description = "Đẹp trai nhưng nghèo",
                ImagePath = "/Images/Avatar.PNG"
            };
            var human2 = new HumanModel
            {
                Name = "Nguyễn Văn A",
                Birthday = new DateTime(1994, 2, 5),
                Address = "Hà Nội",
                Description = "Con nhà giàu",
                ImagePath = "/Images/Avatar.PNG"
            };
            var human3 = new HumanModel
            {
                Name = "Nguyễn Đức B",
                Birthday = new DateTime(1994, 2, 5),
                Address = "Nghệ An",
                Description = "Nghèo bẩm sinh",
                ImagePath = "/Images/Avatar.PNG"
            };
            var human4 = new HumanModel
            {
                Name = "Trần Văn C",
                Birthday = new DateTime(1994, 2, 5),
                Address = "Đà Nẵng",
                Description = "Không ai yêu",
                ImagePath = "/Images/Avatar.PNG"
            };

            var human5 = new HumanModel
            {
                Name = "Nguyễn Vũ D",
                Birthday = new DateTime(1994, 2, 5),
                Address = "Vũng Tàu",
                Description = "Không biết bơi",
                ImagePath = "/Images/Avatar.PNG"
            };

            var humanVM1 = new HumanViewModel(human1);
            var humanVM2 = new HumanViewModel(human2);
            var humanVM3 = new HumanViewModel(human3);
            var humanVM4 = new HumanViewModel(human4);
            var humanVM5 = new HumanViewModel(human5);

            var product1 = new ProductModel
            {
                Name = "Oppo A 83",
                Price = 300,
                Factory = "Aliexpress",
                Provider = "China",
                ExpireDate = new DateTime(2026, 9, 6),
                Address = "Từ Hy Cung, Tử Cấm Thành, Bắc Kinh, Trung Quốc",
                ImagePath = "/Images/MyAddress.PNG",
                Description = "Made in china"
            };
            var product2 = new ProductModel
            {
                Name = "Samsung S26",
                Price = 1200,
                Factory = "Samsung SDV",
                Provider = "Korea",
                ExpireDate = new DateTime(2026, 9, 6),
                Address = "Seoul, Hàn Quốc",
                ImagePath = "/Images/MyAddress.PNG",
                Description = "Dùng tạm"
            };
            var product3 = new ProductModel
            {
                Name = "Iphone 17 ProMax",
                Price = 2000,
                Factory = "Apple Factory",
                Provider = "Apple",
                ExpireDate = new DateTime(2026, 9, 6),
                Address = "Silicon Valley, California",
                ImagePath = "/Images/MyAddress.PNG",
                Description = "Tốn tiền"
            };
            var product4 = new ProductModel
            {
                Name = "Bphone",
                Price = 400,
                Factory = "Bkav Lab",
                Provider = "Bkav",
                ExpireDate = new DateTime(2026, 9, 6),
                Address = "Hà Nội, Việt Nam",
                ImagePath = "/Images/MyAddress.PNG",
                Description = "Bom nguyên tử"
            };

            var productVM1 = new ProductViewModel(product1);
            var productVM2 = new ProductViewModel(product2);
            var productVM3 = new ProductViewModel(product3);
            var productVM4 = new ProductViewModel(product4);

            // dữ liệu mẫu
            var rootA = new NodeModel
            {
                Description = "Mô tả Root A",
                DetailDescription = "Chi tiết Root A",
                ImagePath = "/Images/tooltip.png",
                GifPath = "/Images/pikachu.gif",
                ItemInfo = productVM1
            };
            var childA_1 = new NodeModel { ItemInfo = productVM2, Description = "Mô tả Child A1", DetailDescription = "Chi tiết Child A1", Parent = rootA, GifPath = "Images/pikachu.gif" };
            var childA_2 = new NodeModel { ItemInfo = productVM3, Description = "Mô tả Child A2", DetailDescription = "Chi tiết Child A2", Parent = rootA, GifPath = "Images/test.gif" };
            var grandChildA_1_1 = new NodeModel { ItemInfo = productVM4, Description = "Mô tả GrandChild A1.1", DetailDescription = "Chi tiết GrandChild A1.1", Parent = childA_1 };
            childA_1.Children.Add(grandChildA_1_1);
            rootA.Children.Add(childA_1);
            rootA.Children.Add(childA_2);

            var rootB = new NodeModel { Description = "Mô tả Root B", DetailDescription = "Chi tiết Root B", ImagePath = "/Images/images.png", VideoPath = "Images/lebong.mp4", ItemInfo = humanVM1 };
            var childB_1 = new NodeModel { Description = "Mô tả Child B1", DetailDescription = "Chi tiết Child B1", Parent = rootB, ItemInfo = humanVM2 };
            var childB_2 = new NodeModel { Description = "Mô tả Child B2", DetailDescription = "Chi tiết Child B2", Parent = rootB, ItemInfo = humanVM3 };
            var grandChildB_2_1 = new NodeModel { Description = "Mô tả GrandChild B1.1", DetailDescription = "Chi tiết GrandChild B1.1", Parent = childB_1, ItemInfo = humanVM4 };
            childB_1.Children.Add(grandChildB_2_1);
            rootB.Children.Add(childB_1);
            rootB.Children.Add(childB_2);

            var rootC = new NodeModel
            {
                Description = "Mô tả Root C",
                DetailDescription = "Chi tiết Root C",
                ImagePath = "/Images/tooltip.png",
                GifPath = "/Images/test.gif",
                ItemInfo = humanVM5
            };

            var rootVM1 = new NodeViewModel(rootA, this);
            var rootVM2 = new NodeViewModel(rootB, this);
            var rootVM3 = new NodeViewModel(rootC, this);

            RootNodes = new ObservableCollection<NodeViewModel> { rootVM1, rootVM2, rootVM3 };
        }

        private void InvokeCommand()
        {
            CheckCommand = new RelayCommand(_ => CheckingProcess());
            HoverCommand = new RelayCommand(param => OnHover(param));
            LeaveCommand = new RelayCommand(param => OnLeave(param));
            ExpandAllCommand = new RelayCommand(_ => ExpandCollapseAll(true));
            CollapseAllCommand = new RelayCommand(_ => ExpandCollapseAll(false));
            InvertCommand = new RelayCommand(_ => InvertAll());
            //CopyCommand = new RelayCommand(_ => Copy(), _ => SelectedNode != null);
            //CutCommand = new RelayCommand(_ => Cut(), _ => SelectedNode != null);
            //PasteCommand = new RelayCommand(_ => Paste(), _ => ClipBoardNode != null && SelectedNode != null);
            //DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedNode != null);
            //HelpCommand = new RelayCommand(_ => ShowHelp(), _ => CurrentHoveredNode != null || SelectedNode != null);
        }

        private void CheckStatusOfItemInfo()
        {
            if (SelectedNode != null)
            {
                if (SelectedNode.ItemInfo is HumanViewModel humanVM)
                {
                    IsCheckControlVisible = humanVM.IsChecked;
                    if (IsCheckControlVisible)
                    {
                        CheckContent = "Đã duyệt";
                    }
                    else
                    {
                        CheckContent = "Duyệt";
                    }
                }
                else if (SelectedNode.ItemInfo is ProductViewModel productVM)
                {
                    IsCheckControlVisible = productVM.IsChecked;
                    if (IsCheckControlVisible)
                    {
                        CheckContent = "Đã duyệt";
                    }
                    else
                    {
                        CheckContent = "Duyệt";
                    }
                }
            }
        }

        private void CheckingProcess()
        {
            if (SelectedNode != null)
            {
                IsCheckControlVisible = !IsCheckControlVisible;
                SetIsCheckedPropFromItemInfo(SelectedNode, IsCheckControlVisible);
                if (IsCheckControlVisible)
                {
                    CheckContent = "Đã duyệt";
                }
                else
                {
                    CheckContent = "Duyệt";
                }
            }
        }

        private void SetIsCheckedPropFromItemInfo(NodeViewModel selectedNode, bool isChecked)
        {
            if (selectedNode.ItemInfo is HumanViewModel humanVM)
            {
                humanVM.IsChecked = isChecked;
            }
            else if (selectedNode.ItemInfo is ProductViewModel productVM)
            {
                productVM.IsChecked = isChecked;
            }
        }

        private void OnHover(object? param)
        {
            if (param is NodeViewModel node)
            {
                node.CheckColor = Brushes.Cyan;
            }
        }

        private void OnLeave(object? param)
        {
            if (param is NodeViewModel node)
            {
                node.CheckColor = Brushes.Green;
            }
        }

        public void ShowHelp()
        {
            SelectedNodeImagePath = null;
            SelectedNodeVideoPath = null;
            SelectedNodeGifPath = null;

            if (CurrentHoveredNode != null)
            {
                if (!string.IsNullOrEmpty(CurrentHoveredNode?.VideoPath))
                {
                    SelectedNodeVideoPath = CurrentHoveredNode.VideoPath;
                }
                else if (!string.IsNullOrEmpty(CurrentHoveredNode?.GifPath))
                {
                    SelectedNodeGifPath = CurrentHoveredNode.GifPath;
                }
            }
            else if (SelectedNode != null)
            {
                if (!string.IsNullOrEmpty(SelectedNode?.VideoPath))
                {
                    SelectedNodeVideoPath = SelectedNode.VideoPath;
                }
                else if (!string.IsNullOrEmpty(SelectedNode?.GifPath))
                {
                    SelectedNodeGifPath = SelectedNode.GifPath;
                }
            }
        }

        private int GetLevel(NodeViewModel node)
        {
            int level = 1;
            NodeViewModel? current = node;
            while (current.Parent != null)
            {
                level++;
                current = current.Parent;
            }
            return level;
        }

        private void InvertAll()
        {
            foreach (var node in RootNodes)
                InvertRecursive(node);
        }

        private void InvertRecursive(NodeViewModel node)
        {
            node.IsExpanded = !node.IsExpanded;
            foreach (var child in node.Children)
            {
                InvertRecursive(child);
            }
        }

        private void ExpandCollapseAll(bool expand)
        {
            foreach (var node in RootNodes)
                SetExpandRecursive(node, expand);
        }

        private void SetExpandRecursive(NodeViewModel node, bool expand)
        {
            node.IsExpanded = expand;
            foreach (var child in node.Children)
                SetExpandRecursive(child, expand);
        }

        public void UnMarkedAll()
        {
            foreach (var node in RootNodes)
            {
                node.IsMarked = false;
                foreach (var child in node.Children)
                {
                    child.IsMarked = false;
                }
            }
        }

        public void Copy(MainViewModel mainVM)
        {
            if (SelectedNode == null) return;
            UnMarkedAll();
            //ClipBoardNode = SelectedNode;

            //mainVM.ClipBoardObj = ClipBoardNode;
            mainVM.ClipBoardObj = SelectedNode;

            SelectedNode.IsMarked = true;
            SelectedCopyNode = SelectedNode;
        }

        public void Cut(MainViewModel mainVM)
        {
            if (SelectedNode == null) return;
            UnMarkedAll();
            //ClipBoardNode = SelectedNode;
            //mainVM.ClipBoardObj = ClipBoardNode;
            mainVM.ClipBoardObj = SelectedNode;
            _isCut = true;

            SelectedNode.IsMarked = true;
        }

        public void Paste(MainViewModel mainVM)
        {
            //if (ClipBoardNode == null || SelectedNode == null || mainVM.ClipBoardObj is not NodeViewModel)
            if (SelectedNode == null || mainVM.ClipBoardObj == null || mainVM.ClipBoardObj is not NodeViewModel ClipBoardNode)
            {
                ClearClipboard(mainVM);
                return;
            }

            // Nếu cut chính node đang chọn thì bỏ qua (copy thì vẫn cho phép)
            if (_isCut && ClipBoardNode.Equals(SelectedNode))
            {
                ClearClipboard(mainVM);
                return;
            }

            int sourceLevel = GetLevel(ClipBoardNode);
            int targetLevel = GetLevel(SelectedNode);

            // Chỉ cho phép paste cùng cấp
            if (sourceLevel != targetLevel)
            {
                ClearClipboard(mainVM);
                return;
            }

            var targetCollection = SelectedNode.Parent?.Children ?? RootNodes;
            var sourceCollection = ClipBoardNode.Parent?.Children ?? RootNodes;

            bool isSameColection = false;
            if (ReferenceEquals(sourceCollection, targetCollection))
            {
                isSameColection = true;
            }

            int index = targetCollection.IndexOf(SelectedNode);
            bool isLast = (index == targetCollection.Count - 1);

            if (_isCut)
            {
                int indexToRemove = -1;
                if (!isSameColection)
                {
                    sourceCollection.Remove(ClipBoardNode);
                    ClipBoardNode.Parent = SelectedNode.Parent;
                }
                else
                {
                    indexToRemove = sourceCollection.IndexOf(ClipBoardNode);
                }
                if (isLast)
                    targetCollection.Add(ClipBoardNode);
                else
                    targetCollection.Insert(index + 1, ClipBoardNode);
                if (indexToRemove >= 0)
                {
                    if (indexToRemove < index)
                    {
                        sourceCollection.RemoveAt(indexToRemove);
                    }
                    else
                    {
                        sourceCollection.RemoveAt(indexToRemove + 1);
                    }
                }
            }
            else
            {
                var clone = ClipBoardNode.Clone(this, SelectedNode.Parent);

                if (isLast)
                    targetCollection.Add(clone);
                else
                    targetCollection.Insert(index + 1, clone);
            }
            ClearClipboard(mainVM);
        }

        private void ClearClipboard(MainViewModel mainVM)
        {
            if (SelectedCopyNode != null)
            {
                SelectedCopyNode.IsMarked = false;
            }
            if (mainVM.ClipBoardObj is NodeViewModel node)
            {
                if (node != null)
                {
                    node.IsMarked = false;
                }
            }

            //if (ClipBoardNode != null)
            //{
            //    ClipBoardNode.IsMarked = false;
            //}
            //mainVM.ClipBoardObj = null;
            //ClipBoardNode = null;
            _isCut = false;
        }

        public void Delete()
        {
            if (SelectedNode == null) return;
            if (SelectedNode.Parent != null)
            {
                var tempNode = SelectedNode.Parent;
                SelectedNode.Parent.Children.Remove(SelectedNode);
                SelectedNode = tempNode;
            }
            else
            {
                RootNodes.Remove(SelectedNode);
                SelectedNode = RootNodes.FirstOrDefault();
            }
        }
    }
}