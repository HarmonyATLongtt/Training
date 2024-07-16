using Bai1.ViewModels;
using Bai1.Views;
using Microsoft.Win32;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using WPF_Solution;

namespace Bai1
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields and Properties

        private string pathName;

        public string PathName
        { get => pathName; set { pathName = value; OnPropertyChanged(nameof(PathName)); } }

        private Dictionary<string, ObservableCollection<People>> data;

        public Dictionary<string, ObservableCollection<People>> Data
        { get => data; set { data = value; OnPropertyChanged(nameof(Data)); } }

        private ObservableCollection<string> sheetName;

        public ObservableCollection<string> SheetName
        { get => sheetName; set { sheetName = value; OnPropertyChanged(nameof(SheetName)); } }

        private ObservableCollection<People> peoples;

        public ObservableCollection<People> Peoples
        {
            get { return peoples; }
            set
            {
                peoples = value;
                OnPropertyChanged(nameof(Peoples));
            }
        }

        private string keySearch;

        public string KeySearch
        {
            get { return keySearch; }
            set
            {
                if (keySearch != value)
                {
                    keySearch = value;
                    OnPropertyChanged(nameof(KeySearch));
                    FilterPeoples();
                }
            }
        }

        private string selectedSheet;

        public string SelectedSheet
        {
            get { return selectedSheet; }
            set
            {
                if (selectedSheet != value)
                {
                    selectedSheet = value;
                    OnPropertyChanged(nameof(selectedSheet));
                    LoadData(selectedSheet);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ImportCommand { get; set; } //Command thực hiện thêm dữ liệu từ file excel
        public ICommand ExportCommand { get; set; } //Command thực hiện gọi cửa sổ xuất dữ liệu ra file excel
        public ICommand CellFocusCommand { get; set; } //Command thực hiện đánh dấu hàng được focus của listview, khi focus vào texbox để sửa dữ liệu.

        #endregion Fields and Properties

        #region Constructor

        public MainViewModel()
        {
            sheetName = new ObservableCollection<string>();
            Peoples = new ObservableCollection<People>();
            ImportCommand = new RelayCommand(ImportData, CanImportData);
            ExportCommand = new RelayCommand(ExportData, CanExportData);
            CellFocusCommand = new RelayCommand(FocusRow, CanFocusRow);
        }

        #endregion Constructor

        #region Load and filter data

        private void LoadData(string sheet)
        {
            if (string.IsNullOrEmpty(sheet)) return;
            Peoples = Data[sheet];
            KeySearch = string.Empty;
        }

        private void FilterPeoples()
        {
            if (string.IsNullOrEmpty(KeySearch))
            {
                Peoples = Data[SelectedSheet];
            }
            else
            {
                Peoples = new ObservableCollection<People>(Data[SelectedSheet].Where(p => (p.Name.ToLower()).Contains(KeySearch.ToLower())));
            }
        }

        #endregion Load and filter data

        #region Command

        private bool CanFocusRow(object obj)
        {
            return true;
        }

        private void FocusRow(object obj)
        {
            ListViewItem item = (ListViewItem)obj;

            item.IsSelected = true;
        }

        private bool CanExportData(object obj)
        {
            return !string.IsNullOrEmpty(PathName);
        }

        private void ExportData(object obj)
        {
            ExportWindow window = new ExportWindow();
            var mainWindow = obj as MainWindow;
            window.Owner = mainWindow;
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            ExportViewModel viewModel = new ExportViewModel();
            viewModel.window = window;
            viewModel.SheetName = new List<string>(sheetName);
            viewModel.Data = Data;
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        private bool CanImportData(object obj)
        {
            return true;
        }

        private void ImportData(object obj)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm|All files|*.*";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == true)
            {
                PathName = ofd.FileName.ToString();
                Data = new Dictionary<string, ObservableCollection<People>>();
                sheetName.Clear();
                foreach (string sheet in ExcelHelper.GetSheetsName(PathName))
                {
                    ObservableCollection<People> peoples = new ObservableCollection<People>();
                    peoples = new ObservableCollection<People>(ExcelHelper.GetData<People>(PathName, sheet));
                    Data.Add(sheet, peoples);
                    sheetName.Add(sheet);
                }
                if (sheetName.Count > 0)
                {
                    SelectedSheet = sheetName[0];
                }
            }
        }

        #endregion Command

        #region Other

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Other
    }
}