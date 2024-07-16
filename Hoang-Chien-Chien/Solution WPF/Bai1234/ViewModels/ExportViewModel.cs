using Microsoft.Win32;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;

namespace Bai1.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        #region Fields and Properties

        public Window window;
        private List<string> selectedSheet;
        public List<string> SheetName { get; set; }
        public Dictionary<string, ObservableCollection<People>> Data { get; set; }

        public ICommand ExportDataCommand { get; set; }

        #endregion Fields and Properties

        #region Constructor

        public ExportViewModel()
        {
            selectedSheet = new List<string>();
            Data = new Dictionary<string, ObservableCollection<People>>();
            ExportDataCommand = new RelayCommand(ExportData, CanExportData);
        }

        #endregion Constructor

        #region Command

        private bool CanExportData(object obj)
        {
            return true;
        }

        private void ExportData(object obj)
        {
            ListView listView = obj as ListView;
            if (listView.SelectedItems.Count <= 0)
            {
                MessageBox.Show("VUi lòng chọn sheet muốn xuất dữ liệu", "Thông báo");
                return;
            }
            selectedSheet.Clear();
            foreach (object selectedItem in listView.SelectedItems)
            {
                selectedSheet.Add(selectedItem.ToString());
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Save an Excel File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                Dictionary<string, List<Object>> export = new Dictionary<string, List<Object>>();

                foreach (string sheet in selectedSheet)
                {
                    List<People> peoples = new List<People>();
                    peoples = new List<People>(Data[sheet]);
                    export.Add(sheet, peoples.Cast<object>().ToList());
                }

                if (ExcelHelper.ExportData(filePath, export))
                {
                    MessageBox.Show("Xuất file excel thành công", "Thông báo");
                }
                else
                {
                    MessageBox.Show("Xuất file excel không thành công", "Thông báo");
                }
                window.Close();
            }
        }

        #endregion Command

        #region Other

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Other
    }
}