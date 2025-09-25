using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Windows.Input;
using System.Windows.Media;
using TreeViewProject.ViewModel;

namespace TreeViewProject.Model
{
    public class NodeModel
    {
        public string? Name { get => GetName(); }
        public bool IsExpanded { get; set; } = false;
        public bool IsMarked { get; set; }
        public string? ImagePath { get; set; }
        public string? GifPath { get; set; }
        public string? VideoPath { get; set; }
        public string? Description { get; set; }
        public string? DetailDescription { get; set; }
        public List<NodeModel> Children { get; set; } = new List<NodeModel>();
        public NodeModel? Parent { get; set; }

        public Brush CheckColor { get; set; } = Brushes.Green;

        public object? ItemInfo { get; set; }

        private string? GetName()
        {
            if (ItemInfo != null)
            {
                if (ItemInfo is HumanViewModel humnaVM)
                {
                    return humnaVM.Name;
                }
                else if (ItemInfo is ProductViewModel productVM)
                {
                    return productVM.Name;
                }
            }
            return null;
        }

        public NodeModel Clone(NodeModel? parent = null)
        {
            var clone = new NodeModel()
            {
                IsExpanded = IsExpanded,
                IsMarked = false,
                ImagePath = ImagePath,
                GifPath = GifPath,
                VideoPath = VideoPath,
                Description = Description,
                DetailDescription = DetailDescription,
                CheckColor = CheckColor,
                Parent = parent,
                Children = new List<NodeModel>()
            };
            if (ItemInfo is HumanViewModel humnaVM)
            {
                clone.ItemInfo = humnaVM.Clone();
            }
            else if (ItemInfo is ProductViewModel productVM)
            {
                clone.ItemInfo = productVM.Clone();
            }
            foreach (var child in Children)
            {
                var cloneChild = child.Clone(clone);
                clone.Children.Add(cloneChild);
            }

            return clone;
        }
    }
}