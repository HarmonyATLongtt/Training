using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace WPF_Ex.Model
{
    public class ItemModel
    {
        public string SheetName { get; set; }
        public ObservableCollection<Person> People { get; set; }

        public ItemModel(string sheetName, IEnumerable<Person> people)
        {
            SheetName = sheetName;
            People = new ObservableCollection<Person>(people);
        }
    }
}
