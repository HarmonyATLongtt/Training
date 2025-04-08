using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using FirstCommand.ViewModel;

namespace FirstCommand.View
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : Window
    {
        public ScheduleView(Document doc, ViewSchedule schedule)
        {
            InitializeComponent();
            DataContext = new ScheduleViewModel(doc, schedule);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var columnIndex = e.Column.DisplayIndex;
            var rowIndex = e.Row.GetIndex();
            var editedValue = ((TextBox)e.EditingElement).Text;

            var viewmodel = (ScheduleViewModel)this.DataContext;

            foreach (var cellInfo in viewmodel.CellInfos)
            {
                if (cellInfo.colCellTable == columnIndex && cellInfo.rowCellTable == rowIndex + 1)
                {
                    cellInfo.value = editedValue.ToString();
                    cellInfo.isChanged = true;
                }
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var viewModel = this.DataContext as ScheduleViewModel;

            if (viewModel != null)
            {
                if (viewModel.ListFieldCombinedOrCaculated.Contains(e.PropertyName))
                {
                    e.Column.IsReadOnly = true;

                    var style = new Style(typeof(DataGridCell));
                    var headerStyle = new Style(typeof(DataGridColumnHeader));
                    style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightGray));
                    headerStyle.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty, Brushes.LightGray));

                    e.Column.CellStyle = style;
                    e.Column.HeaderStyle = headerStyle;
                }
                if (viewModel.DoubleColumns.Contains(e.PropertyName))
                {
                    System.Windows.Data.Binding binding = new System.Windows.Data.Binding(e.PropertyName)
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.LostFocus, // Kích hoạt validation khi mất focus
                        ValidatesOnDataErrors = true // Bắt lỗi từ IDataErrorInfo (nếu DataTable implement)
                    };
                    binding.ValidationRules.Add(new DoubleValidationRule());

                    // Tạo DataGridTextColumn và gán Binding
                    DataGridTextColumn textColumn = new DataGridTextColumn();
                    textColumn.Header = e.Column.Header;
                    textColumn.Binding = binding;

                    // Thay thế cột tự động tạo bằng cột đã cấu hình validation
                    e.Column = textColumn;
                }
            }
        }

        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var viewModel = this.DataContext as ScheduleViewModel;
            int rowIndex = e.Row.GetIndex();
            if (viewModel != null && viewModel.RowIndexes.Contains(rowIndex))
            {
                e.Cancel = true;
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var viewModel = this.DataContext as ScheduleViewModel;
            int rowIndex = e.Row.GetIndex();
            if (viewModel != null && viewModel.RowIndexes.Contains(rowIndex))
            {
                e.Row.Background = Brushes.LightGray;
            }
        }
    }

    public class DoubleValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            //if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            //return new ValidationResult(false, "Không được để trống");

            if (!double.TryParse(value.ToString(), out _))
            {
                MessageBox.Show("Giá trị phải là số!", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);

                //return new ValidationResult(false, "Phải là số");
            }

            return ValidationResult.ValidResult;
        }
    }
}