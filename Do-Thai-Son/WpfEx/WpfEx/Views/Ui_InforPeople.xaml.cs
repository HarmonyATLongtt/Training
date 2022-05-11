using System.Data;
using System.Windows;
using WpfEx.Model;
using WpfEx.ViewModels;

namespace WpfEx.Views
{
    /// <summary>
    /// Interaction logic for Ui_InforPeople.xaml
    /// </summary>
    public partial class Ui_InforPeople : Window
    {
        public Ui_InforPeople()
        {
            InitializeComponent();
            MainModel mainModel = new MainModel(new System.Collections.Generic.List<DataTable>());
            MainViewModel mainViewModel = new MainViewModel(mainModel);
            this.DataContext = mainViewModel;
        }
    }
}