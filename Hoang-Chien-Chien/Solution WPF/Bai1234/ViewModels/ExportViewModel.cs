using Microsoft.Win32;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bai1.ViewModels
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        #region Fields and Properties

        public Window window;
        public ListView LsvSheetName { get; set; }


        private List<string> selectedSheet;

        public List<string> SheetName { get; set; }
        public List<string> SelectedSheet { get => selectedSheet; set => selectedSheet = value; }


        private bool chekboxState;

        public bool ChekboxState
        { 
            get => chekboxState;
            set 
            { 
                chekboxState = value; OnPropertyChanged(nameof(ChekboxState));
            }
        }

        private string selectedItem;

        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                if (LsvSheetName.SelectedItems.Count == SheetName.Count())
                {
                    ChekboxState = false;
                }
            }
        }

        public Dictionary<string, ObservableCollection<People>> Data { get; set; }

        public ICommand ExportDataCommand { get; set; } // Command thực hiện xuất dữ liệu từ Sheet đã chọn ra file excel
        public ICommand SelectAllCommand { get; set; } // Command thực hiện thao tác chọn hoặc hủy chọn tất cả các Sheet
        

        #endregion Fields and Properties

        #region Constructor

        public ExportViewModel()
        {
            SelectedSheet = new List<string>();
            Data = new Dictionary<string, ObservableCollection<People>>();
            ExportDataCommand = new RelayCommand(ExportData, CanExportData);
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);
        }

        #endregion Constructor

        #region Command: Execute and CanExecute

        private bool CanSelectAll(object obj)
        {
            return SheetName.Count() > 0;
        }

        private void SelectAll(object obj)
        {
            ListView listView = obj as ListView;
            LsvSheetName = listView;
            foreach (object item in listView.Items)
            {
                if (ChekboxState)
                {
                    listView.SelectedItems.Add(item);
                }
                else
                {
                    listView.SelectedItems.Remove(item);
                }
            }
        }

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
            SelectedSheet.Clear();
            foreach (object selectedItem in listView.SelectedItems)
            {
                SelectedSheet.Add(selectedItem.ToString());
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

                foreach (string sheet in SelectedSheet)
                {
                    List<People> peoples = new List<People>(Data[sheet]);
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

        #region Others

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Others
    }
}