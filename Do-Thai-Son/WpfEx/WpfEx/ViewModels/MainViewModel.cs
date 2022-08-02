using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfEx.Model;
using WpfEx.ViewModels.Base;
using WpfEx.Views;

namespace WpfEx.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Properties

        #region ex

        //public MainModel Model { get; private set; }
        //private ObservableCollection<SheetViewModel> _sheetViewModels;

        //public ObservableCollection<SheetViewModel> SheetViewModels
        //{
        //    get => _sheetViewModels;
        //    set => SetProperty(ref _sheetViewModels, value);
        //}

        //private SheetViewModel _selectedSheetViewModel;

        //public SheetViewModel SelectedSheetViewModel
        //{
        //    get => _selectedSheetViewModel;
        //    set => SetProperty(ref _selectedSheetViewModel, value);
        //}

        //public const double TOLERANCE = 1.0e-5;
        //public string btnImport { get; set; }

        //private readonly MainModel _mainModel;

        //public ObservableCollection<string> SheetNames
        //{
        //    get => _mainModel.SheetNames;
        //    set
        //    {
        //        RaisePropertyChanged(nameof(SheetNames));
        //    }
        //}

        //public string CurrentSheetName
        //{
        //    get => _mainModel.CurrentSheetName;
        //    set
        //    {
        //        _mainModel.CurrentSheetName = value;
        //        RaisePropertyChanged(nameof(CurrentSheetName));
        //    }
        //}

        //public string FileName
        //{
        //    get => _mainModel.FileName;
        //    set
        //    {
        //        _mainModel.FileName = value;
        //        RaisePropertyChanged(nameof(FileName));
        //    }
        //}

        //private DataTable _dataTable;

        //public DataTable DataTable
        //{
        //    get => _dataTable;
        //    set
        //    {
        //        _dataTable = value;
        //        SetProperty(ref _dataTable, value, nameof(DataTable));
        //    }
        //}

        #endregion ex

        private const int SALARY = 17000000;
        public MainModel _mainModel;

        public ObservableCollection<SheetModel> SheetModels
        {
            get => _mainModel.Sheets;
            set
            {
                _mainModel.Sheets = value;
                RaisePropertyChanged(nameof(SheetModels));
            }
        }

        private ObservableCollection<SheetViewModel> _sheetViewModles;

        public ObservableCollection<SheetViewModel> SheetViewModels
        {
            get => _sheetViewModles;
            set => SetProperty(ref _sheetViewModles, value);
        }

        private SheetViewModel _selectSheetViewModel;

        public SheetViewModel SelectSheetViewModel
        {
            get => _selectSheetViewModel;
            set => SetProperty(ref _selectSheetViewModel, value);
        }

        public string FileName
        {
            get => _mainModel.FileName;
            set
            {
                _mainModel.FileName = value;
                RaisePropertyChanged(nameof(FileName));
            }
        }

        private SalaryPersonViewModel _salaryPersonViewModel;

        public SalaryPersonViewModel SalaryPersonViewModel
        {
            get => _salaryPersonViewModel;
            set => SetProperty(ref _salaryPersonViewModel, value);
        }

        private AddIteamDataGridViewModle _AddIteamViewModle;

        public AddIteamDataGridViewModle AddIteamViewModle
        {
            get => _AddIteamViewModle;
            set => SetProperty(ref _AddIteamViewModle, value);
        }

        #endregion Properties

        #region Command and event

        public ICommand ImportCommand { get; set; }

        public ICommand ExportCommand { get; set; }

        public ICommand AddCommand { get; set; }

        public ICommand RemoveCommand { get; set; }

        public ICommand OKCommand { get; set; }

        #endregion Command and event

        public MainViewModel(MainModel mainModel)
        {
            _mainModel = mainModel;
            _sheetViewModles = new ObservableCollection<SheetViewModel>();
            foreach (var sheetModel in SheetModels)
            {
                _sheetViewModles.Add(new SheetViewModel(sheetModel));
            }
            InitCommand();
        }

        //public void AddRecordCommandInvoke()
        //{
        //    List<string> newReocrd = GetData();
        //    _selectedSheetViewModel.AddReocrd(newReocrd);
        //}

        #region Init

        private void InitCommand()
        {
            ImportCommand = new RelayCommand<object>(ImportCommandInvoke);
            ExportCommand = new RelayCommand<object>(ExportCommandInvoke);
            AddCommand = new RelayCommand<object>(AddCommandInvoke);
            RemoveCommand = new RelayCommand<object>(RemoveInvoke);
            OKCommand = new RelayCommand<object>(OkInvoke);
        }

        #endregion Init

        #region command implementations and event handlers

        /// <summary>
        /// Import Command
        /// </summary>
        /// <param name="obj"></param>
        private void ImportCommandInvoke(object obj)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "(*.xlsx)|*.xlsx";

            if (file.ShowDialog().Value
                && !CheckUsingFile(file.FileName))
            {
                // Open workbook
                XLWorkbook xLWorkbook = new XLWorkbook(file.FileName);

                List<DataTable> dataTables = new List<DataTable>();
                foreach (IXLWorksheet xLWorksheet in xLWorkbook.Worksheets)
                {
                    DataTable dataTable = GetData(xLWorksheet);
                    dataTable.TableName = xLWorksheet.Name;
                    if (dataTable != null)
                        dataTables.Add(dataTable);
                }
                _mainModel = new MainModel(dataTables);
                FileName = file.FileName;
                SheetViewModels = new ObservableCollection<SheetViewModel>();
                _mainModel.Sheets.ToList().ForEach(item => SheetViewModels.Add(new SheetViewModel(item)));
                SelectSheetViewModel = SheetViewModels.FirstOrDefault();
            }
        }

        private void ExportCommandInvoke(object obj)
        {
            if (_mainModel.FileName != null)
            {
                string filePath = GetFilePath();
                if (!string.IsNullOrEmpty(filePath)
                    && !CheckUsingFile(filePath))
                {
                    IXLWorkbook wb = new XLWorkbook();
                    if (SheetViewModels?.Count > 0)
                    {
                        foreach (SheetViewModel data in SheetViewModels)
                        {
                            IXLWorksheet ws = wb.Worksheets.Add(data.DataTable, data.Name);
                        }
                    }
                    wb.SaveAs(filePath);
                }
            }
            else
                MessageBox.Show("Import file before export");
        }

        /// <summary>
        /// remove iteam in datagrid
        /// </summary>
        /// <param name="obj"></param>
        private void RemoveInvoke(object obj)
        {
            if (obj is DataGrid dataGrid)
            {
                if (_mainModel.FileName != null)
                {
                    _selectSheetViewModel.DataTable.Rows.RemoveAt(dataGrid.SelectedIndex);

                    // auto set number oder when remove row has column is "STT"
                    if (_selectSheetViewModel.DataTable.Columns[0].ColumnName == "STT")
                    {
                        int index = 1;
                        foreach (DataRow row in _selectSheetViewModel.DataTable.Rows)
                        {
                            if (!string.IsNullOrEmpty(row[0].ToString()))
                            {
                                row[0] = index;
                                index++;
                            }
                        }
                    }

                    RaisePropertyChanged(nameof(SelectSheetViewModel));
                }
                else
                    MessageBox.Show("Not data to remove");
            }
        }

        private void AddCommandInvoke(object obj)
        {
            if (obj is DataGrid)
            {
                if (_mainModel.FileName != null)
                {
                    DataTable addInforTable = new DataTable();

                    // Add column for Salary Table
                    for (int i = 0; i < SelectSheetViewModel.DataTable.Columns.Count; i++)
                    {
                        addInforTable.Columns.Add(SelectSheetViewModel.DataTable.Columns[i].ColumnName, SelectSheetViewModel.DataTable.Columns[i].DataType);
                    }

                    Ui_AddIteamDataGrid dialogAddTable = new Ui_AddIteamDataGrid();
                    AddIteamDataGridModle addIteamModel = new AddIteamDataGridModle(addInforTable);
                    AddIteamDataGridViewModle addIteamDataGridViewModel = new AddIteamDataGridViewModle(addIteamModel);
                    dialogAddTable.DataContext = addIteamDataGridViewModel;

                    if (dialogAddTable.ShowDialog() == true)
                    {
                        if (addInforTable != null
                            && addInforTable.Rows.Count > 0)
                        {
                            SelectSheetViewModel.AddIteam(addInforTable.Rows);
                            RaisePropertyChanged(nameof(SelectSheetViewModel));
                        }
                    }
                }
                else
                    MessageBox.Show("Import file before export");
            }
        }

        /// <summary>
        /// Calculate salary of person
        /// </summary>
        /// <param name="obj"></param>
        private void OkInvoke(object obj)
        {
            if (obj is DataGrid dataGrid)
            {
                if (SelectSheetViewModel.DataTable.TableName == "Teacher"
                   || SelectSheetViewModel.DataTable.TableName == "Employee")
                {
                    int? index = dataGrid.Columns.Single(c => c.Header.ToString() == "Thu Nhập").DisplayIndex;
                    if (index != null)
                    {
                        DataTable salaryTable = new DataTable();

                        // Add column for Salary Table
                        for (int i = 0; i < SelectSheetViewModel.DataTable.Columns.Count; i++)
                        {
                            salaryTable.Columns.Add(SelectSheetViewModel.DataTable.Columns[i].ColumnName);
                        }

                        // Add Row for Salary Table
                        foreach (DataRow row in _selectSheetViewModel.DataTable.Rows)
                        {
                            if (ToInt(row[(int)index].ToString(), out int salary)
                               && salary >= SALARY)
                            {
                                salaryTable.Rows.Add(row.ItemArray);
                            }
                        }

                        Ui_SalaryTable dialogSalary = new Ui_SalaryTable();
                        SalaryPersonModel salaryModel = new SalaryPersonModel(salaryTable);
                        SalaryPersonViewModel = new SalaryPersonViewModel(salaryModel);

                        dialogSalary.DataContext = SalaryPersonViewModel;
                        dialogSalary.ShowDialog();
                    }
                }
            }
        }

        /// <summary>
        /// Create Datatable from data of worksheet
        /// </summary>
        /// <param name="xLWorksheet"></param>
        /// <returns></returns>
        private DataTable GetData(IXLWorksheet xLWorksheet)
        {
            //Create a new DataTable.
            DataTable dt = new DataTable();

            //Loop through the Worksheet rows.
            bool firstRow = true;
            List<Type> typeDatables = GetTypeForDataColumns(xLWorksheet.Rows());

            foreach (IXLRow row in xLWorksheet.Rows())
            {
                //Use the first row to add columns to DataTable.
                if (firstRow && typeDatables?.Count == row.Cells().Count())
                {
                    int index = 0;
                    foreach (IXLCell cell in row.Cells())
                    {
                        dt.Columns.Add(cell.Value.ToString(), typeDatables[index]);
                        index++;
                    }

                    firstRow = false;
                }
                else
                {
                    if (row.Cells().Count() > 0)
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (IXLCell cell in row.Cells())
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            i++;
                        }
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Get type for datable from file excel
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        private List<Type> GetTypeForDataColumns(IXLRows rows)
        {
            List<Type> types = new List<Type>();

            int i = 0;
            foreach (IXLRow row in rows)
            {
                if (i != 0)
                {
                    foreach (IXLCell cell in row.Cells())
                        types.Add(ConvertType(cell.DataType));

                    if (types?.Count > 0)
                        return types;
                }
                i++;
            }
            return types;
        }

        /// <summary>
        /// convert type for datable
        /// </summary>
        /// <param name="typeExcel"></param>
        /// <returns></returns>
        private Type ConvertType(XLDataType typeExcel)
        {
            switch (typeExcel)
            {
                case XLDataType.Boolean:
                    return typeof(bool);

                case XLDataType.Text:
                    return typeof(string);

                case XLDataType.DateTime:
                    return typeof(string);

                case XLDataType.TimeSpan:
                    return typeof(string);

                case XLDataType.Number:
                    return typeof(double);
            }
            return null;
        }

        /// <summary>
        /// get file path
        /// </summary>
        /// <returns></returns>
        private string GetFilePath()
        {
            string outputPath = string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
            };

            if (saveFileDialog.ShowDialog().Value)
            {
                outputPath = saveFileDialog.FileName;
            }
            return outputPath;
        }

        /// <summary>
        /// check existing file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckUsingFile(string path)
        {
            if (File.Exists(path))
            {
                if (IsFileInUse(path))
                {
                    MessageBox.Show("File is using");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check file is using
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileInUse(string path)
        {
            FileStream stream = null;
            try
            {
                FileInfo file = new FileInfo(path);
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return false;
        }

        /// <summary>
        /// Conver string to int
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool ToInt(string input, out int output)
        {
            output = 0;
            if (int.TryParse(input, out int space))
            {
                output = space;
                return true;
            }
            return false;
        }

        #endregion command implementations and event handlers
    }
}