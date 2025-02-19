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

namespace FirstCommand.View
{
    /// <summary>
    /// Interaction logic for AddElevationView.xaml
    /// </summary>
    public partial class AddElevationView : Window
    {
        public string Elevator { get; set; }
        public Level Level { get; set; }

        private Document doc { get; set; }

        public AddElevationView(Document doc)
        {
            this.doc = doc;
            InitializeComponent();
            LoadLevels();
        }

        private void LoadLevels()
        {
            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .OrderBy(l => l.Name)
                .ToList();

            cbBoxLevels.ItemsSource = levels;
            cbBoxLevels.DisplayMemberPath = "Name";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Elevator = txbElevator.Text;
            Level = cbBoxLevels.SelectedItem as Level;

            this.DialogResult = true;
            this.Close();
        }
    }
}