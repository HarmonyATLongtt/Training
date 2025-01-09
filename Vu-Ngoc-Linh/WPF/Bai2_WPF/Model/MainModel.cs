using Bai2_WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_WPF.Model
{
    public class MainModel
    {
        public string FilePath { get; set; }
        public ItemModel SelectedItem { get; set; }
        public ObservableCollection<ItemModel> Items { get; set; }
        public MainModel()
        {
            FilePath = string.Empty;
        }
    }
}
