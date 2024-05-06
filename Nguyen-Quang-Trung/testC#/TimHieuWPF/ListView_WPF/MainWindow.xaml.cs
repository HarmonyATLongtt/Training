using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListView_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool sx;

        public MainWindow()
        {
            sx = false;
            List<SanPham> ListSP = new List<SanPham>
            {
                new SanPham() {STT = 1, Ten = "San pham A", Gia = 1000000, Loai=1},
                new SanPham() {STT = 2, Ten = "San pham B", Gia = 1300005, Loai= 1},
                new SanPham() {STT = 3, Ten = "San pham C", Gia = 3982171, Loai=2},
                new SanPham() {STT = 4, Ten = "San pham D", Gia = 9217133, Loai=1},
                new SanPham() {STT = 5, Ten = "San pham E", Gia = 1000000, Loai=2},
            };
            InitializeComponent();
            listInfor.ItemsSource = ListSP;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listInfor.ItemsSource);
            view.Filter = SPFilter;
            //PropertyGroupDescription groupDescription = new PropertyGroupDescription("Loai");
            //view.GroupDescriptions.Add(groupDescription);

            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listInfor.ItemsSource);
            //view.SortDescriptions.Add(new SortDescription("Gia", ListSortDirection.Descending));
            //view.SortDescriptions.Add(new SortDescription("Ten", ListSortDirection.Descending));
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader header = sender as GridViewColumnHeader;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listInfor.ItemsSource);
            //view.SortDescriptions.Add(new SortDescription("Gia", ListSortDirection.Descending));
            if (sx)
            {
                //view.SortDescriptions.Remove(new SortDescription(header.Content.ToString(), ListSortDirection.Ascending));
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(header.Content.ToString(), ListSortDirection.Descending));
            }
            else
            {
                //view.SortDescriptions.Remove(new SortDescription(header.Content.ToString(), ListSortDirection.Descending));
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(header.Content.ToString(), ListSortDirection.Ascending));
            }
            sx = !sx;
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(listInfor.ItemsSource).Refresh();
        }

        private bool SPFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;
            else
                return ((item as SanPham).Ten.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    public class SanPham
    {
        public int STT { get; set; }
        public string Ten { get; set; }
        public int Gia { get; set; }
        public int Loai { get; set; }
    }
}