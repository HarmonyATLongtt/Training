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
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand.View
{
    /// <summary>
    /// Interaction logic for ViewComboboxWindow.xaml
    /// </summary>
    public partial class ViewComboboxWindow : Window
    {
        public Autodesk.Revit.DB.View SelectedView { get; set; }
        private Document doc { get; set; }

        public ViewComboboxWindow(Document doc)
        {
            this.doc = doc;
            InitializeComponent();
            LoadViews();
        }

        private void LoadViews()
        {
            var views = new FilteredElementCollector(doc)
               .OfClass(typeof(Autodesk.Revit.DB.View))
               .OfType<Autodesk.Revit.DB.View>()
               .Where(v => !v.IsTemplate)
               .ToList();
            comboBoxViews.ItemsSource = views;
            comboBoxViews.DisplayMemberPath = "Name";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedView = comboBoxViews.SelectedItem as Autodesk.Revit.DB.View;
            this.DialogResult = true;
            this.Close();
        }
    }
}