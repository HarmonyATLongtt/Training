using System.Collections.ObjectModel;
using System.IO;

namespace WPF_Ex.Model
{
    public class MainModel
    {
        public string FilePath { get; set; }
        public ObservableCollection<object> People { get; set; } 
        public object SelectedPerson { get; set; }

        public MainModel()
        {
            People = new ObservableCollection<object>();
            SelectedPerson = People.FirstOrDefault();

        }
    }
}
