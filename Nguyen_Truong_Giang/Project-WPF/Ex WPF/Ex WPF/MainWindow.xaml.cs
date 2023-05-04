using Ex_WPF.ModelView;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ex_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public object data { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new BaseViewModel();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Hủy bỏ và đóng màn hình view
                Close();
            }
        }

        private void Cell_MouseMove(object sender, MouseEventArgs e)
        {
            var rowIndex = DataGrid.SelectedIndex;

            if (sender is DataGridCell cell)
            {
                // Xác định hàng và cột của cell được di chuyển
                var row = cell.DataContext as ObservableCollection<object>;
                //var rowIndex = (row != null) ? data.IndexOf(row) : -1;
                var columnIndex = cell.Column.DisplayIndex;

                // Gán giá trị của thuộc tính Background của cell được di chuyển bằng màu LightCyan
                var vm = DataContext as BaseViewModel;
                if (vm != null)
                {
                    var brush = vm.MouseHover;
                    cell.Background = brush;
                }
            }
        }

        private void Cell_MouseLeave(object sender, MouseEventArgs e)
        {
            var rowIndex = DataGrid.SelectedIndex;

            if (sender is DataGridCell cell)
            {
                // Xác định hàng và cột của cell được di chuyển
                var row = cell.DataContext as ObservableCollection<object>;
                //var rowIndex = (row != null) ? Data.IndexOf(row) : -1;
                var columnIndex = cell.Column.DisplayIndex;
                cell.Background = Brushes.White;
            }
        }
    }

}
