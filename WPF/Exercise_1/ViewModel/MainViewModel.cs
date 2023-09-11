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
    public class MainViewModel
    {
        public ObservableCollection<Data> data { get; set; }
        public ICommand ImportFileCommand { get; set; }
        public ObservableCollection<string> WorkSheetName { get; set; }

        public ICommand ExportFileCommand { get; set; }

        private ListView _listView;
        public ListView ListViewControl
        {
            get { return _listView; }
            set { _listView = value; }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                NotifyPropertyChanged(nameof(SelectedIndex));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
            UpdateData();
        }

        private void UpdateData()
        {
            ListViewControl.ItemsSource = data[SelectedIndex].data.DefaultView;
        }

        public MainViewModel()
        {
            DataManager x = new DataManager();
            data = DataManager.GetData();
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
            string filePath = "";

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Excel | *.xlsx";

            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            try
            {
                using(var wb = new XLWorkbook())
                {
                    foreach(var i in WorkSheetName)
                    { 
                        var ws = wb.AddWorksheet(i);
                        ws.ColumnWidth = 25;
                        ws.FirstCell().InsertTable(data[WorkSheetName.IndexOf(i)].data);
                    }
                    wb.SaveAs(filePath);
                }
                MessageBox.Show("Export file successful...", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanImportFile(object obj)
        {
            return true;
        }

        private void ImportFile(object obj)
        {
            string filePath = "";
            WorkSheetName.Clear();
            data.Clear();
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
                    foreach (var ws in wb.Worksheets)
                    {
                        Data x = new Data();
                        WorkSheetName.Add(ws.Name);
                        var firstRow = ws.FirstRowUsed();
                        foreach (var cell in firstRow.Cells())
                        {
                            x.data.Columns.Add(cell.Value.ToString());
                        }

                        var rows = ws.RowsUsed().Skip(1);
                        foreach (var row in rows)
                        {
                            var newRow = x.data.NewRow();
                            for (int i = 0; i < x.data.Columns.Count; i++)
                            {
                                newRow[i] = row.Cell(i + 1).Value.ToString();
                            }
                            x.data.Rows.Add(newRow);
                        }
                        data.Add(x);
                    }
                    if(!WorkSheetName.Contains("Student") || !WorkSheetName.Contains("Teacher") || !WorkSheetName.Contains("Employee"))
                    {
                        WorkSheetName.Clear();
                        data.Clear();
                        MessageBox.Show("Can't not import this file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                MessageBox.Show("Import file successful...", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("Can't not import this file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }
    }
}
