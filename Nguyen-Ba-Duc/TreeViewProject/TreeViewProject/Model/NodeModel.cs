using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using TreeViewProject.ViewModel;

namespace TreeViewProject.Model
{
    public class NodeModel
    {
        public string? Name { get; set; }
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
    }
}