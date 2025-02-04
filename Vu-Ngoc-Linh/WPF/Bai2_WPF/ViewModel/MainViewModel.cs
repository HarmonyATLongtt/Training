using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Bai2_WPF.Command;
using Bai2_WPF.Model;
using Microsoft.Win32;
using OfficeOpenXml;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            Items.Clear();
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    DataTable dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();
                    dt.TableName = worksheet.Name;
                    ItemModel itemModel;
                    if (dt.TableName == "Student") itemModel = LoadData<Student>(dt, studentmapping);
                    else if(dt.TableName == "Teacher") itemModel = LoadData<Teacher> (dt, teachermapping);
                    else itemModel = LoadData<Employee>(dt, employeemapping);

                    Items.Add(new ItemViewModel(itemModel));
                }
                SelectedItem = Items.FirstOrDefault();
            }
        }
        private Dictionary<string, string> studentmapping = new Dictionary<string, string>
        {
            {"STT", "STT" },
            { "Name", "Tên" },
            { "Age", "Tuổi" },
            {"ID", "ID" },
            {"DOB", "Ngày sinh" },
            { "School", "Trường" },
            {"Class", "Lớp" }
        };
        private Dictionary<string, string> teachermapping = new Dictionary<string, string>
        {
            {"STT", "STT" },
            { "Name", "Tên" },
            { "Age", "Tuổi" },
            {"ID", "ID" },
            {"DOB", "Ngày sinh" },
            { "School", "Trường" },
            {"Class", "Lớp" },
            {"Income", "Thu nhập" }
        };
        private Dictionary<string, string> employeemapping = new Dictionary<string, string>
        {
            {"STT", "STT" },
            { "Name", "Tên" },
            { "Age", "Tuổi" },
            {"ID", "ID" },
            {"DOB", "Ngày sinh" },
            { "Company", "Công ty" },
            {"Team", "Phòng ban" },
            {"Role", "Chức vụ" },
            {"Income", "Thu nhập" }
        };
        private ItemModel LoadData<T>(DataTable dt, Dictionary<string, string> columnMapping) where T : Person, new()
        {
            var list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                var obj = new T();
                foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
                {
                    if (!property.CanWrite) continue;

                    // Lấy tên cột từ ánh xạ
                    // xử lý nếu tên cột là tiếng việt hoặc tiếng anh
                    string columnName, columnName2 = "";
                    if (columnMapping.ContainsKey(property.Name))
                    {
                        columnName = columnMapping[property.Name];
                        columnName2 = property.Name;
                    }
                    else
                    {
                        columnName = property.Name;
                    }

                    if (row.Table.Columns.Contains(columnName) || row.Table.Columns.Contains(columnName2))
                    {
                        var value = row.Table.Columns.Contains(columnName)
                            ? row[columnName]
                            : row[columnName2];
                        if (value == DBNull.Value) continue;
                        try
                        {
                            if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(obj, int.TryParse(value.ToString(), out int intValue) ? intValue : 0);
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                property.SetValue(obj, double.TryParse(value.ToString(), out double doubleValue) ? doubleValue : 0.0);
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                if (value is DateTime dateValue) //xử lý cho thuộc tính DOB
                                {
                                    property.SetValue(obj, dateValue.ToString("dd/MM/yyyy"));
                                }
                                else
                                    property.SetValue(obj, value.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                list.Add(obj);
            }
            return new ItemModel(typeof(T).Name, list.Cast<Person>());
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
                    Debug.WriteLine($"Sheet: {item.SheetName}, People count: {item.Model.People.Count}");
                    
                    var dataTable = ConvertToDataTable(item.People, item.SheetName);
                    var worksheet = package.Workbook.Worksheets.Add(item.SheetName);
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                }
                package.Save();
                MessageBox.Show("Data exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+"\n"+ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable ConvertToDataTable<T>(IEnumerable<T> data, string tableName)
        {
            DataTable dt = new DataTable();
            var getType = data.First().GetType();

            var declaring = getType.DeclaringType;
            var baseType = getType.BaseType;
            var reflected = getType.ReflectedType;
            var undelying = getType.UnderlyingSystemType;

            Dictionary<string, string> columnMapping = GetColumnMapping(getType);
            foreach (var property in undelying.GetProperties())
            {
                string columnName = property.Name;
                dt.Columns.Add(columnName, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (var item in data)
            {
                var row = dt.NewRow();
                foreach (var property in getType.GetProperties())
                {
                    string columnName = property.Name;
                    row[columnName] = property.GetValue(item) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            dt.TableName = tableName;
            return dt;
        }

        private Dictionary<string, string> GetColumnMapping(Type objectType)
        {
            if (objectType == typeof(Student)) return studentmapping;
            if (objectType == typeof(Teacher)) return teachermapping;
                return employeemapping;
        }
    }
}