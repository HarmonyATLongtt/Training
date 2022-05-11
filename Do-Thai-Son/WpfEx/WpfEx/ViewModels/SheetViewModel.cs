using System.Collections.Generic;
using System.Data;
using WpfEx.Model;

namespace WpfEx.ViewModels
{
    public class SheetViewModel : BindableBase
    {
        public SheetModel SheetModel;

        public string Name
        {
            get => SheetModel.Name;
            set
            {
                SheetModel.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public DataTable DataTable
        {
            get => SheetModel.Data;
            set => SetProperty(ref SheetModel.Data, value, nameof(DataTable));
        }

        public SheetViewModel(SheetModel Model)
        {
            this.SheetModel = Model;
        }

        public void AddIteam(DataRowCollection dataRows)
        {
            SheetModel.AddIteam(dataRows);
        }

        public void AddReocrd(List<string> cellDatas)
        {
            // them logic de validate
            SheetModel.AddRecord(cellDatas);
        }
    }
}