using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Exercise_1.Commands;
using Exercise_1.Models;
using Microsoft.Win32;
using ClosedXML;
using ClosedXML.Excel;
using System.Windows.Controls;
using System.ComponentModel;
using Exercise_1;

namespace Exercise_1.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Declare an observable collection name 'data' to follow change of data when it be changed on view
        public ObservableCollection<Data> data { get; set; }
        // Declare an Icommand to binding it for even click of button Import
        public ICommand ImportFileCommand { get; set; }
        // Declare an observable collection to get and store name of sheets, follow change it when selected name of sheets in combo box
        public ObservableCollection<string> WorkSheetName { get; set; }
        // Declare an Icommand to binding it for even click of button Export
        public ICommand ExportFileCommand { get; set; }
        // Declare a data grid to binding from data grid in view, change data on it (change items source when change value in combo box, when change value in cell)
        private DataGrid _dataGrid;
        // Data grid control is an instance of _dataGrid, it use for getting value and setting value of _dataGrid, (encapsulate)
        public DataGrid DataGridControl
        {
            get { return _dataGrid; }
            set { _dataGrid = value; }
        }
        // Declare a variable store index of combo box is selected to change view corresponding
        private int _selectedIndex;
        // Feature the same DataGridControl
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                // Notify change value of selected index for view
                NotifyPropertyChanged(nameof(SelectedIndex));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
            // When selected index be changed, reupdate data
            UpdateData();
        }

        // Update data function
        private void UpdateData()
        {
            try
            {
                // Reassign items source of data grid
                DataGridControl.ItemsSource = data[SelectedIndex]._Data.DefaultView;
            }
            catch
            {

            }
        }
        // Constructor
        public MainViewModel()
        {
            // Get data
            data = DataManager.GetData();
            // Create an instance of observable type string
            WorkSheetName = new ObservableCollection<string>();

            ImportFileCommand = new RelayCommand(ImportFile, CanImportFile);
            ExportFileCommand = new RelayCommand(ExportFile, CanExportFile);
        }

        private bool CanExportFile(object obj)
        {
            return true;
        }

        private void ExportFile(object obj)
        {

            // Declare and assign filePath = "";
            string filePath = "";
            // Create a dialog type SaveFileDialog to get path to save file
            SaveFileDialog dialog = new SaveFileDialog();
            // Use filter to save file with format *.xlsx;
            dialog.Filter = "Excel | *.xlsx";
            // Check dialog was showed?
            if (dialog.ShowDialog() == true)
            {
                // Assign file path for dialog.FileName (path)
                filePath = dialog.FileName;
            }

            try
            {
                // Create an instance of XLWorkbook
                using(var wb = new XLWorkbook())
                {
                    // Use for loop to browse sheets available im view (data table, combo box)
                    foreach(var i in WorkSheetName)
                    { 
                        // Assign name of worksheet corresponding
                        var ws = wb.AddWorksheet(i);
                        // Set width of column
                        ws.ColumnWidth = 20;
                        // Insert data to table start in first cell
                        ws.FirstCell().InsertTable(data[WorkSheetName.IndexOf(i)]._Data);
                    }
                    // Save file to file path
                    wb.SaveAs(filePath);
                }
                // Notify export file successful
                MessageBox.Show("Export file successful...", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                // Notify error if have
                MessageBox.Show(ex.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanImportFile(object obj)
        {
            return true;
        }

        private void ImportFile(object obj)
        {
            // Declare and assign file path = ""
            string filePath = "";
            // Clear all data have in worksheet variable (in case import fail, data will be stored)
            WorkSheetName.Clear();
            // Clear all data have in data
            data.Clear();
            // Create a OpenFileDialog to get path to open file
            OpenFileDialog dialog = new OpenFileDialog();
            
            dialog.Filter = "Excel| *.xlsx";

            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            try
            {
                using (var wb = new XLWorkbook(filePath))
                {
                    // Browse all worksheet have in workbook
                    foreach (var ws in wb.Worksheets)
                    {
                        // Create new Data to get data in each sheet
                        Data x = new Data();
                        // Get name of each sheet
                        WorkSheetName.Add(ws.Name);
                        // Get first row of sheet, this row include header of table
                        var firstRow = ws.FirstRowUsed();
                        // Browse each cell in first row
                        foreach (var cell in firstRow.Cells())
                        {
                            // Assign each value for data column
                            x._Data.Columns.Add(cell.Value.ToString());
                        }
                        // Get all rows of sheet skip row 1
                        var rows = ws.RowsUsed().Skip(1);
                        // Take each row in list row
                        foreach (var row in rows)
                        {
                            // Create new row in data  of each data
                            var newRow = x._Data.NewRow();
                            // Browse all cell in row
                            for (int i = 0; i < x._Data.Columns.Count; i++)
                            {
                                // Assign value of cell i by value of cell i + i in row
                                newRow[i] = row.Cell(i + 1).Value.ToString();
                            }
                            // Add row for _Data
                            x._Data.Rows.Add(newRow);
                        }
                        // Add sheet for data list
                        data.Add(x);
                    }
                    // Check if in file not contain "Student" sheet or "Teacher" sheet or "Employee" sheet through error message
                    if (!WorkSheetName.Contains("Student") || !WorkSheetName.Contains("Teacher") || !WorkSheetName.Contains("Employee"))
                    {
                        // Clear all data in list name of sheet
                        WorkSheetName.Clear();
                        // Clear all data
                        data.Clear();
                        // Notify can not import file
                        MessageBox.Show("Can't not import this file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        // Return
                        return;
                    }
                }
                // Notify import file successful
                MessageBox.Show("Import file successful...", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                // Show view 0 of combo of
                SelectedIndex = 0;
            }
            catch
            {
                // Through error dialog and message error
                MessageBox.Show("Can't not import this file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Function handle edit value in data grid
        public void HandleCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            // Check if edit is execute
            if(e.EditAction == DataGridEditAction.Commit)
            {
                // Get index of column
                var column = e.Column.DisplayIndex;
                // Get index of row
                var row = e.Row.GetIndex();
                // Get content be changed
                var cellContent = e.EditingElement as TextBox;
                // Check if content is not null
                if (cellContent != null)
                {
                    // Create a variable store value of text edit
                    string editValue = cellContent.Text;
                    // Assign value of text was edited for data table
                    data[SelectedIndex]._Data.Rows[row][column] = editValue;
                    //Notify change for view
                    OnPropertyChange(nameof(DataGridControl));
                }
            }
        }

        private void OnPropertyChange(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }
    }
}
