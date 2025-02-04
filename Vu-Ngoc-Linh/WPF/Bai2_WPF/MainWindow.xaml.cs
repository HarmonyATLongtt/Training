﻿using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bai2_WPF.Model;
using Bai2_WPF.ViewModel;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Bai2_WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainModel model = new MainModel();
            MainViewModel mainViewModel = new MainViewModel(model);
            this.DataContext = mainViewModel;

        }
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void dataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid.Items?.Count > 0)
            {
                var sampleItem = dataGrid.Items.GetItemAt(0);
                var itemType = sampleItem.GetType();

                if (itemType != null)
                {
                    dataGrid.Columns.Clear();
                    var props = GetAllProperties(itemType);
                    foreach (var prop in props)
                    {
                        var col = new DataGridTextColumn
                        {
                            Header = prop.Name,
                            Binding = new Binding(prop.Name)
                        };
                        dataGrid.Columns.Add(col);
                    }
                }
            }
        }
        private IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            var propertyList = new List<PropertyInfo>();

            while (type != null && type != typeof(object))
            {
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                propertyList.InsertRange(0, props);
                type = type.BaseType;
            }

            return propertyList;
        }
    }
}