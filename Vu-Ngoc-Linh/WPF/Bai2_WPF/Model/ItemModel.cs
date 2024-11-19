using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_WPF.Model
{
    public class ItemModel
    {
        public string SheetName { get; set; }
        public ObservableCollection<Person> People { get; set; }
    }
}
