using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace WpfEx.Model
{
    public class MainModel
    {
        public string FileName { get; set; }
        public ObservableCollection<SheetModel> Sheets { get; set; }

        public MainModel(IEnumerable<DataTable> tables)
        {
            Sheets = new ObservableCollection<SheetModel>();
            if (tables?.Count() > 0)
            {
                foreach (DataTable table in tables)
                {
                    SheetModel sheetModel = new SheetModel(table);
                    Sheets.Add(sheetModel);
                }
            }
        }
    }
}