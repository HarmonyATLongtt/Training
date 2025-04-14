using System;
using System.Collections.Generic;
using System.Data;
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
        private object copiedValue = null;
        private int copiedColumnIndex = -1;

        //private Stack<UndoAction> undoStack { get; set; }
        private HashSet<(int, int)> readOnlyCells = new HashSet<(int, int)>();

        public ScheduleView(Document doc, ViewSchedule schedule, List<CellIsReadOnly> CellIsReadOnlys, List<RowInfo> RowInfoList)
        {
            InitializeComponent();
            DataContext = new ScheduleViewModel(doc, schedule, CellIsReadOnlys, RowInfoList);
            UpdateReadOnlyCells();
            //undoStack = new Stack<UndoAction>();
            //dataGrid.PreviewKeyDown += dataGrid_PreviewKeyDown;
            //dataGrid.PreviewKeyDown += dataGrid_PreviewKeyDown_ForPaste;
        }

        private void UpdateReadOnlyCells()
        {
            var viewmodel = (ScheduleViewModel)this.DataContext;
            if (viewmodel != null)
            {
                int rowCount = viewmodel.RowCount;
                int colCount = viewmodel.ColCount;
                readOnlyCells.UnionWith(viewmodel.cellIsReadOnlys);

                foreach (var (name, col) in viewmodel.ListFieldCombinedOrCaculated)
                {
                    for (int row = 0; row < rowCount; row++)
                    {
                        readOnlyCells.Add((row + 1, col));
                    }
                }
                foreach (int row in viewmodel.RowIndexes)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        readOnlyCells.Add((row + 1, col));
                    }
                }
            }
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
                    if (cellInfo.value != editedValue.ToString().Trim())
                    {
                        cellInfo.value = editedValue.ToString();
                        cellInfo.isChanged = true;
                    }
                }
            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var viewModel = this.DataContext as ScheduleViewModel;

            if (viewModel != null)
            {
                List<string> nameCol = new List<string>();
                foreach (var (name, col) in viewModel.ListFieldCombinedOrCaculated)
                {
                    nameCol.Add(name);
                }
                if (nameCol.Contains(e.PropertyName))
                {
                    e.Column.IsReadOnly = true;

                    var style = new Style(typeof(DataGridCell));
                    //var headerStyle = new Style(typeof(DataGridColumnHeader));
                    style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightGray));
                    //headerStyle.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty, Brushes.LightGray));

                    e.Column.CellStyle = style;
                    //e.Column.HeaderStyle = headerStyle;
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
            int columnIndex = e.Column.DisplayIndex;
            if (viewModel != null)
            {
                if (viewModel.RowIndexes.Contains(rowIndex))
                {
                    e.Cancel = true;
                }
                //if (viewModel.cellIsReadOnlys.Contains((rowIndex + 1, columnIndex)))
                //{
                //    e.Cancel = true;
                //}
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var viewModel = this.DataContext as ScheduleViewModel;
            int rowIndex = e.Row.GetIndex();
            if (viewModel.RowIndexes.Contains(rowIndex))
            {
                e.Row.Background = Brushes.DarkGray;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var dataGrid = sender as DataGrid;
                for (int colIndex = 0; colIndex < dataGrid.Columns.Count; colIndex++)
                {
                    var cellContent = dataGrid.Columns[colIndex].GetCellContent(e.Row);
                    if (cellContent != null)
                    {
                        var cell = FindParent<DataGridCell>(cellContent);
                        if (viewModel.cellIsReadOnlys.Contains((rowIndex + 1, colIndex)))
                        {
                            //cell.Background = Brushes.LightBlue;
                            cell.Foreground = Brushes.Red;
                            cell.IsEnabled = false;
                        }
                        if (viewModel.RowIndexes.Contains(rowIndex))
                        {
                            cell.Background = Brushes.DarkGray;
                        }
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T tParent) return tParent;
            return FindParent<T>(parent);
        }

        private void dataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var cellInfo = dataGrid.CurrentCell;
                if (cellInfo != null && cellInfo.IsValid)
                {
                    var columnIndex = cellInfo.Column.DisplayIndex;
                    var row = cellInfo.Item as DataRowView;

                    if (row != null)
                    {
                        copiedValue = row.Row[columnIndex];
                        copiedColumnIndex = columnIndex;
                    }
                }

                e.Handled = true; // Ngăn hành vi mặc định nếu muốn
            }
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (copiedValue != null && copiedColumnIndex >= 0)
                {
                    foreach (var item in dataGrid.SelectedItems)
                    {
                        var row = item as DataRowView;
                        if (row != null)
                        {
                            //var oldValue = row.Row[copiedColumnIndex];
                            //undoStack.Push(new UndoAction
                            //{
                            //    Row = row,
                            //    ColumnIndex = copiedColumnIndex,
                            //    OldValue = oldValue
                            //});
                            int rowIndex = dataGrid.Items.IndexOf(row);
                            int colIndex = copiedColumnIndex;
                            if (readOnlyCells.Contains((rowIndex + 1, colIndex)))
                                continue;
                            row.Row[copiedColumnIndex] = copiedValue;
                        }
                    }

                    dataGrid.Items.Refresh(); // Cập nhật lại UI nếu cần
                }
                e.Handled = true; // Ngăn Ctrl+V mặc định
            }
            //if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            //{
            //    if (undoStack.Count > 0)
            //    {
            //        var action = undoStack.Pop();
            //        action.Row.Row[action.ColumnIndex] = action.OldValue;
            //        dataGrid.Items.Refresh();
            //    }
            //    e.Handled = true;
            //}
        }

        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            var cellInfo = dataGrid.CurrentCell;
            var row = cellInfo.Item as DataRowView;

            if (row != null)
            {
                copiedColumnIndex = cellInfo.Column.DisplayIndex;
                copiedValue = row.Row[copiedColumnIndex];
            }
        }

        private void MenuItem_Paste_Click(object sender, RoutedEventArgs e)
        {
            if (copiedValue != null && copiedColumnIndex >= 0)
            {
                foreach (var item in dataGrid.SelectedItems)
                {
                    if (item is DataRowView row)
                    {
                        //var oldValue = row.Row[copiedColumnIndex];
                        //undoStack.Push(new UndoAction
                        //{
                        //    Row = row,
                        //    ColumnIndex = copiedColumnIndex,
                        //    OldValue = oldValue
                        //});
                        int rowIndex = dataGrid.Items.IndexOf(row);
                        int colIndex = copiedColumnIndex;
                        if (readOnlyCells.Contains((rowIndex + 1, colIndex)))
                            continue;
                        row.Row[copiedColumnIndex] = copiedValue;
                    }
                }

                dataGrid.Items.Refresh(); // Cập nhật UI
            }
        }

        //private void dataGrid_PreviewKeyDown_ForPaste(object sender, KeyEventArgs e)
        //{
        //if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        //{
        //    if (copiedValue != null && copiedColumnIndex >= 0)
        //    {
        //        foreach (var item in dataGrid.SelectedItems)
        //        {
        //            var row = item as DataRowView;
        //            if (row != null)
        //            {
        //                row.Row[copiedColumnIndex] = copiedValue;
        //            }
        //        }

        //        dataGrid.Items.Refresh(); // Cập nhật lại UI nếu cần
        //    }

        //    e.Handled = true; // Ngăn Ctrl+V mặc định
        //}
        //}

        //private void dataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //var dep = (DependencyObject)e.OriginalSource;

        //// Tìm đến DataGridCell
        //var cell = FindParent<DataGridCell>(dep);
        //if (cell != null)
        //{
        //    cell.Focus(); // Đưa cell đó làm CurrentCell (quan trọng)

        //    var row = FindParent<DataGridRow>(cell);
        //    if (row != null)
        //    {
        //        row.IsSelected = true; // Đảm bảo dòng đang được chọn
        //    }
        //}
        //}
    }

    //public class UndoAction
    //{
    //    public DataRowView Row { get; set; }
    //    public int ColumnIndex { get; set; }
    //    public object OldValue { get; set; }
    //}

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