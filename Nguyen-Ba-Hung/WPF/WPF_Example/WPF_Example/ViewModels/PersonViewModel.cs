using WPF_Example.Models;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using Microsoft.Win32;
using System.IO;
using ExcelDataReader;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Example.ViewModels
{
    public class PersonViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<DataTable> sheets;
        private DataTable records;

        public ObservableCollection<DataTable> Sheets 
        {
            get
            {
                return sheets;
            }
            set
            {
                if (sheets != value)
                {
                    sheets = value;
                    OnPropertyChanged(nameof(Sheets));
                }
            }
        }
        public DataTable Records
        {
            get
            {
                return records;
            }
            set
            {
                if(records != value)
                {
                    records = value;
                    OnPropertyChanged(nameof(Records));
                }
            }
        }

        public PersonViewModel()
        {
            sheets = new ObservableCollection<DataTable>();

            ImportFile = new RelayCommand<object>(
                (impF) => true,
                (impF) => ImportExcel()
                );
            ExportFile = new RelayCommand<object>(
                (expF) => true,
                (expF) => ExportExcel()
                );
            OnKeyEscape = new RelayCommand<object>(KeyEscape);
        }

        public ICommand ImportFile { get;set;}
        public ICommand ExportFile { get;set;}
        //public ICommand MouseHover { get;set;}
        //public ICommand MouseLeave { get;set;}
        public ICommand OnKeyEscape { get;set;}

        public void ImportExcel()
        {
            if(sheets != null)
            {
                sheets.Clear();
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if(dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;

                using(var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using(var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                    {
                        var config = new ExcelDataSetConfiguration
                        {
                            ConfigureDataTable = _ => new ExcelDataTableConfiguration
                            {
                                UseHeaderRow = true
                            }
                        };

                        var dataSet = reader.AsDataSet(config).Tables;

                        foreach(DataTable data in dataSet)
                        {
                            sheets.Add(data);
                        }

                        Records = sheets.FirstOrDefault();
                        reader.Close();
                    }
                    stream.Close();
                    MessageBox.Show("Imported");
                }
            }
        }

        public void ExportExcel()
        {
            if(sheets.FirstOrDefault() == null)
            {
                MessageBox.Show("No data");
            }
            else
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

                if (saveDialog.ShowDialog() == true)
                {
                    using (ExcelPackage package = new ExcelPackage())
                    {
                        foreach (DataTable sheet in sheets)
                        {
                            ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(sheet.TableName);
                            for (int i = 0; i < sheet.Columns.Count; i++)
                            {
                                workSheet.Cells[1, i + 1].Value = sheet.Columns[i].ColumnName;
                            }

                            for (int i = 0; i < sheet.Rows.Count; i++)
                            {
                                for (int j = 0; j < sheet.Columns.Count; j++)
                                {
                                    workSheet.Cells[i + 2, j + 1].Value = sheet.Rows[i][j];
                                }
                            }
                        }
                        package.SaveAs(new FileStream(saveDialog.FileName, FileMode.Create));
                        package.Dispose();
                    }
                    MessageBox.Show("Exported");
                }
            }
            
        }

        public void KeyEscape(object obj)
        {
            var args = obj as KeyEventArgs;
            if (args.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

    }
}
