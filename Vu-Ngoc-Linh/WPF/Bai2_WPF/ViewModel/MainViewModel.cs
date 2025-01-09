using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Bai2_WPF.Command;
using Bai2_WPF.Model;
using Microsoft.Win32;
using OfficeOpenXml;

namespace Bai2_WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainModel Model { get; private set; }

        public string FilePath
        {
            get => Model.FilePath;
            set
            {
                Model.FilePath = value;
                OnPropertyChanged();
            }
        }

        private ItemViewModel _selectedItem;
        public ItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ItemViewModel> _items;
        public ObservableCollection<ItemViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ICommand ImportData { get; set; }
        public ICommand ExportData { get; set; }

        public MainViewModel(MainModel model)
        {
            Model = model;
            Items = new ObservableCollection<ItemViewModel>();
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanImportFile(object obj)
        {
            return true;
        }

        private void ImportFile(object obj)
        {
            string filePath = "";
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Invalid file.");
                return;
            }

            FilePath = filePath;
            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    DataTable dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();
                    dt.TableName = worksheet.Name;
                    var itemModel = LoadData(dt);
                    Items.Add(new ItemViewModel(itemModel));

                }
                SelectedItem = Items.FirstOrDefault();
            }
        }

        private T MapDataRowToObject<T>(DataRow row) where T : new()
        {
            T obj = new T();
            foreach (var property in typeof(T).GetProperties())
            {
                if (row.Table.Columns.Contains(property.Name) && property.CanWrite)
                {
                    var value = row[property.Name];
                    property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                }
            }
            return obj;
        }

        private ItemModel LoadData(DataTable dt)
        {
            string name = dt.TableName;
            var list = new List<object>(); 

            if (name == "Student")
            {
                foreach (DataRow row in dt.Rows)
                {
                    var student = MapDataRowToObject<Student>(row);
                    list.Add(student);
                }
            }
            else if (name == "Teacher")
            {
                foreach (DataRow row in dt.Rows)
                {
                    var teacher = MapDataRowToObject<Teacher>(row);
                    teacher.TaxCoe = TaxData.GetTaxCoe(teacher.Age, teacher.Income);
                    teacher.Tax = teacher.GetTax();
                    list.Add(teacher);
                }
            }
            else if (name == "Employee")
            {
                foreach (DataRow row in dt.Rows)
                {
                    var employee = MapDataRowToObject<Employee>(row);
                    employee.TaxCoe = TaxData.GetTaxCoe(employee.Age, employee.Income);
                    employee.Tax = employee.GetTax();
                    list.Add(employee);
                }
            }

            return new ItemModel(name, list);  
        }
        private bool CanExportFile(object obj)
        {
            return true;
        }

        private void ExportFile(object obj)
        {
            if (!Items.Any())
            {
                MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                using var package = new ExcelPackage(new FileInfo(saveFileDialog.FileName));

                foreach (var item in Items)
                {
                    var dataTable = ConvertToDataTable(item.Model.People, item.SheetName);
                    var worksheet = package.Workbook.Worksheets.Add(item.SheetName);
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                }

                package.Save();
                MessageBox.Show("Data exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable ConvertToDataTable<T>(IEnumerable<T> data, string tableName)
        {
            DataTable dt = new DataTable();

            foreach (var property in typeof(T).GetProperties())
            {
                dt.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (var item in data)
            {
                var row = dt.NewRow();
                foreach (var property in typeof(T).GetProperties())
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            dt.TableName = tableName;
            return dt;
        }
    }
}
