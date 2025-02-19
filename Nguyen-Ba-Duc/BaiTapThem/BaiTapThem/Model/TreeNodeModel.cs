using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.ViewModel;

namespace BaiTapThem.Model
{
    public class TreeNodeModel : BaseViewModel
    {
        private bool _isChecked = false;

        private TreeNodeModel? _parent;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));

                    // Check/Uncheck tất cả các node con
                    if (Children != null)
                    {
                        foreach (var child in Children)
                        {
                            child.IsChecked = value;
                        }
                    }
                    if (_parent != null)
                    {
                        _parent.VerifyCheckState();
                    }
                }
            }
        }

        public string? Name { get; set; }

        public ObservableCollection<TreeNodeModel> Children { get; set; }

        public TreeNodeModel(string name, TreeNodeModel? parent = null)
        {
            Name = name;
            _parent = parent;
            Children = new ObservableCollection<TreeNodeModel>();
        }

        private void VerifyCheckState()
        {
            bool state = false;
            for (int i = 0; i < Children.Count; ++i)
            {
                bool current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = false;
                    break;
                }
            }
            IsChecked = state;

            //bool state = Children[0].IsChecked;
            //bool kq = false;
            //for (int i = 0; i < Children.Count; i++)
            //{
            //    bool currentState = Children[i].IsChecked;
            //    if (state != currentState)
            //    {
            //        kq = true;
            //        break;
            //    }
            //}
            //if (kq)
            //{
            //    IsChecked = false;
            //}
            //if (state)
            //{
            //    IsChecked = true;
            //}
            if (_parent != null)
            {
                _parent.VerifyCheckState();
            }
        }
    }

    public class Family
    {
        public Family()
        {
            this.Members = new ObservableCollection<FamilyMember>();
        }

        public string Name { get; set; }

        public ObservableCollection<FamilyMember> Members { get; set; }
    }

    public class FamilyMember
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }
}