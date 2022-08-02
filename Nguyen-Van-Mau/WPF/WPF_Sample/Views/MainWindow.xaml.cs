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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Sample.Models;
using WPF_Sample.ViewModels;

namespace WPF_Sample.Views

{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainModel _mainModel = new MainModel();
        public MainViewModel MainVM { get; set; }

        public MainWindow()
        {
            MainVM = new MainViewModel(_mainModel, this);
            InitializeComponent();
            // this.Icon = Properties.Resources.Manager;
            this.DataContext = MainVM;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
           var v1 = (sender as CheckBox).IsChecked.Value;
            if (dgv.Items.Count > 0)
            {
                List<object> list = new List<object>();

                foreach(var item in dgv.SelectedItems)
                {
                    list.Add(item as ClassViewModel);
                }
                
                Dictionary<int,object> indexs = new Dictionary<int,object>();

                list.ForEach(item => indexs.Add(MainVM.ClassViewModels.IndexOf(item as ClassViewModel), item));

                this.Title= indexs.Min(item => item.Key).ToString();


                //var dg = sender as DataGrid;
                //if (dg == null) return;
                //var index = dg.SelectedIndex;
                ////here we get the actual row at selected index
                //DataGridRow row = dg.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;

                ////here we get the actual data item behind the selected row
                //var item = dg.ItemContainerGenerator.ItemFromContainer(row);

               

                foreach (var item in dgv.SelectedItems)
                {
                    (item as ClassViewModel).AllowEditable = v1;
                }

            }
       
        }
    }
}