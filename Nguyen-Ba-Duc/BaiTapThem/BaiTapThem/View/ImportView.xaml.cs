using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BaiTapThem.ViewModel;
using Microsoft.Extensions.Primitives;

namespace BaiTapThem.View
{
    /// <summary>
    /// Interaction logic for ImportView.xaml
    /// </summary>
    public partial class ImportView : Window
    {
        public ImportView()
        {
            InitializeComponent();
            DataContext = new ImportViewModel();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var columnIndex = e.Column.DisplayIndex;
            var rowIndex = e.Row.GetIndex();
            //var editedValue = (e.EditingElement as TextBox).Text;
            var editedValue = ((TextBox)e.EditingElement).Text;

            var viewmodel = (ImportViewModel)this.DataContext;
            int selectedComboBox = viewmodel.SelectedIndex;

            foreach (var excelCellMapping in viewmodel.ExcelCellMappings)
            {
                if (excelCellMapping.UiColumn == columnIndex && excelCellMapping.UiRow == rowIndex
                    && excelCellMapping.SheetIndex == selectedComboBox && excelCellMapping.Value != editedValue)
                {
                    excelCellMapping.IsChanged = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CongTyMeView congTyMeView = new CongTyMeView();
            this.Close();
            congTyMeView.Show();
        }
    }
}