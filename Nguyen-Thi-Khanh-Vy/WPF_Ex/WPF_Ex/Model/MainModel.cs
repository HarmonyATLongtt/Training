using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace WPF_Ex.Model
{
    public class MainModel
        {
            public string FileName { get; set; }
            public ItemModel SelectedItem { get; set; }
            public ObservableCollection<ItemModel> Items { get; set; }

            public MainModel()
            {
                FileName = string.Empty;
                
            }
        }
    }
