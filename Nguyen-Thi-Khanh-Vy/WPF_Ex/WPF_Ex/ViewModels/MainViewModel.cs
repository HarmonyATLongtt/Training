using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using OfficeOpenXml;
using WPF_Ex.Model;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace WPF_Ex.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public MainModel Model { get; private set; }
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private ObservableCollection<ItemViewModel> _items;
        public ObservableCollection<ItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private ItemViewModel _selectedItem;
        public ItemViewModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public ICommand LoadCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public MainViewModel()
        {
            Model = new MainModel();
            Items = new ObservableCollection<ItemViewModel>();
            LoadCommand = new RelayCommand(LoadExcelFile, CanImportFile);
            ExportCommand = new RelayCommand(ExportToExcel, CanExportFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private Dictionary<string, string[]> _worksheetFieldOrders = new Dictionary<string, string[]>();

        private bool CanImportFile(object obj) => true;

        private void LoadExcelFile(object obj)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            FileName = dialog.FileName;

            using (var package = new ExcelPackage(new FileInfo(FileName)))
            {
                Items.Clear();

                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    List<ItemModel> items = null;
                    string[] fields = null;

                    switch (worksheet.Name)
                    {
                        case "Student":
                            fields = new string[] { "ID", "Name", "Age", "Class", "School" };
                            items = LoadItems<Student>(worksheet, fields);
                            break;

                        case "Teacher":
                            fields = new string[] { "ID", "Name", "Age", "School", "Income", "TaxCoe" };
                            items = LoadItems<Teacher>(worksheet, fields);
                            break;

                        case "Employee":
                            fields = new string[] { "ID", "Name", "Age", "Income", "TaxCoe", "JobTitle", "Company" };
                            items = LoadItems<Employee>(worksheet, fields);
                            break;
                    }

                    if (fields != null)
                    {
                        _worksheetFieldOrders[worksheet.Name] = fields;
                    }

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            Items.Add(new ItemViewModel(item));
                        }
                    }
                }
                SelectedItem = Items.FirstOrDefault();
            }
        }


        private List<ItemModel> LoadItems<T>(ExcelWorksheet worksheet, string[] properties) where T : Person, new()
        {
            var list = new List<T>();
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var obj = new T();
                var type = typeof(T);

                for (int col = 1; col <= properties.Length; col++)
                {
                    var propertyName = properties[col - 1];
                    var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                    if (property != null)
                    {
                        var cellValue = worksheet.Cells[row, col].Text;

                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            try
                            {
                                if (property.PropertyType == typeof(int))
                                {
                                    property.SetValue(obj, int.TryParse(cellValue, out int intValue) ? intValue : 0);
                                }
                                else if (property.PropertyType == typeof(double))
                                {
                                    property.SetValue(obj, double.TryParse(cellValue, out double doubleValue) ? doubleValue : 0.0);
                                }
                                else if (property.PropertyType == typeof(string))
                                {
                                    property.SetValue(obj, cellValue);
                                }
                                else if (property.PropertyType == typeof(DateTime))
                                {
                                    if (DateTime.TryParse(cellValue, out DateTime dateValue))
                                    {
                                        property.SetValue(obj, dateValue);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Lỗi khi ánh xạ dữ liệu tại dòng {row}, cột {col}: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                list.Add(obj);
            }
            return new List<ItemModel> { new ItemModel(typeof(T).Name, list.Cast<Person>()) };
        }

        private bool CanExportFile(object obj) => true;

        private void ExportToExcel(object parameter)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var package = new ExcelPackage())
                    {
                        foreach (var item in Items)
                        {
                            var worksheet = package.Workbook.Worksheets.Add(item.SheetName);

                            if (_worksheetFieldOrders.TryGetValue(item.SheetName, out var fields))
                            {
                                for (int i = 0; i < fields.Length; i++)
                                {
                                    worksheet.Cells[1, i + 1].Value = fields[i];
                                }

                                int row = 2;
                                foreach (var person in item.People)
                                {
                                    for (int i = 0; i < fields.Length; i++)
                                    {
                                        var property = person.GetType().GetProperty(fields[i]);
                                        var value = property?.GetValue(person);

                                        worksheet.Cells[row, i + 1].Value = value ?? "N/A";
                                    }
                                    row++;
                                }

                                worksheet.Cells["A1:Z1"].Style.Font.Bold = true;
                                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                            }
                        }

                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MessageBox.Show("Xuất dữ liệu thành công.", "Xuất dữ liệu", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
