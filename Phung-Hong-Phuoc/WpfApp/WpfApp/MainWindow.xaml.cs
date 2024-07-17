using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeOpenXml;
using System;
using System.Collections.ObjectModel;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private ExcelPackage package;
        private Dictionary<string, ObservableCollection<People>> sheetData;
        public ObservableCollection<People> PeopleList { get; set; }

        public class People : INotifyPropertyChanged
        {
            private string name;
            private int age;
            private int id;
            private string adr;
            private double taxCoe;

            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            public int Age
            {
                get { return age; }
                set
                {
                    age = value;
                    OnPropertyChanged(nameof(Age));
                }
            }

            public int ID
            {
                get { return id; }
                set
                {
                    id = value;
                    OnPropertyChanged(nameof(ID));
                }
            }

            public string Adr
            {
                get { return adr; }
                set
                {
                    adr = value;
                    OnPropertyChanged(nameof(Adr));
                }
            }

            public double TaxCoe
            {
                get { return taxCoe; }
                set
                {
                    taxCoe = value;
                    OnPropertyChanged(nameof(TaxCoe));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            sheetData = new Dictionary<string, ObservableCollection<People>>();
            DataContext = this;
        }

        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Excel files(*.xlsx)| *.xlsx";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    string filepath = ofd.FileName;
                    LoadDataSheet(filepath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing file: " + ex.Message);
                }
            }
        }

        private void LoadDataSheet(string filename)
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                cbSheetNames.Items.Clear();
                package = new ExcelPackage(new FileInfo(filename));
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    cbSheetNames.Items.Add(worksheet.Name);
                }
                if (cbSheetNames.Items.Count > 0)
                {
                    cbSheetNames.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data sheet: " + ex.Message);
            }
        }

        private void cbSheetNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (package == null || cbSheetNames.SelectedIndex < 0)
                return;

            var selectedSheetName = cbSheetNames.SelectedItem.ToString();
            if (sheetData.ContainsKey(selectedSheetName))
            {
                PeopleList = sheetData[selectedSheetName];
            }
            else
            {
                var sheet = package.Workbook.Worksheets[selectedSheetName];
                ObservableCollection<People> peopleList = new ObservableCollection<People>();

                for (var rowNumber = 2; rowNumber <= sheet.Dimension.End.Row; rowNumber++)
                {
                    var people = new People
                    {
                        Name = sheet.Cells[rowNumber, 1].Text,
                        Age = int.TryParse(sheet.Cells[rowNumber, 2].Text, out int age) ? age : 0,
                        ID = int.TryParse(sheet.Cells[rowNumber, 3].Text, out int id) ? id : 0,
                        Adr = sheet.Cells[rowNumber, 4].Text,
                        TaxCoe = double.TryParse(sheet.Cells[rowNumber, 5].Text, out double taxCoe) ? taxCoe : 0.0
                    };

                    peopleList.Add(people);
                }

                sheetData[selectedSheetName] = peopleList;
                PeopleList = peopleList;
            }
            dataGrid.ItemsSource = PeopleList;
        }

        private void btn_Export_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel files(*.xlsx)| *.xlsx";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    string filepath = sfd.FileName;
                    ExportDataSheet(filepath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting file: " + ex.Message);
                }
            }
        }

        private void ExportDataSheet(string filename)
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    foreach (var sheetName in sheetData.Keys)
                    {
                        var worksheet = package.Workbook.Worksheets.Add(sheetName);
                        var peopleList = sheetData[sheetName];
                        worksheet.Cells[1, 1].Value = "Name";
                        worksheet.Cells[1, 2].Value = "Age";
                        worksheet.Cells[1, 3].Value = "ID";
                        worksheet.Cells[1, 4].Value = "Adr";
                        worksheet.Cells[1, 5].Value = "TaxCoe";

                        for (int i = 0; i < peopleList.Count; i++)
                        {
                            worksheet.Cells[i + 2, 1].Value = peopleList[i].Name;
                            worksheet.Cells[i + 2, 2].Value = peopleList[i].Age;
                            worksheet.Cells[i + 2, 3].Value = peopleList[i].ID;
                            worksheet.Cells[i + 2, 4].Value = peopleList[i].Adr;
                            worksheet.Cells[i + 2, 5].Value = peopleList[i].TaxCoe;
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    }

                    package.SaveAs(new FileInfo(filename));
                    MessageBox.Show("Export successful!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting data sheet: " + ex.Message);
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var cell = e.EditingElement as TextBox;
                if (cell != null)
                {
                    var binding = cell.GetBindingExpression(TextBox.TextProperty);
                    binding.UpdateSource();
                }
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}