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
    /// Interaction logic for InputMaterialView.xaml
    /// </summary>
    public partial class InputMaterialView : Window
    {
        private Document _doc { get; set; }

        public List<MaterialAndLevel> materialAndLevels { get; set; }

        public InputMaterialView(Document doc)
        {
            this._doc = doc;
            InitializeComponent();
            LoadLevelsAndMaterials();
        }

        private void LoadLevelsAndMaterials()
        {
            var levels = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .OrderBy(l => l.Name)
                .ToList();
            if (levels.Count < 3)
            {
                MessageBox.Show("Cần ít nhất 3 Level để phân đoạn.");
                this.DialogResult = false;
                this.Close();
            }

            var materials = new FilteredElementCollector(_doc)
                .OfClass(typeof(Material))
                .OfType<Material>()
                .Where(m => m.Name == "Red Material" || m.Name == "Green Material" || m.Name == "Blue Material")
                .OrderBy(m => m.Name)
                .ToList();

            cbBoxLevelsTop.ItemsSource = levels;
            cbBoxLevelsTop.DisplayMemberPath = "Name";

            cbBoxLevelsMiddle.ItemsSource = levels;
            cbBoxLevelsMiddle.DisplayMemberPath = "Name";

            cbBoxLevelsBottom.ItemsSource = levels;
            cbBoxLevelsBottom.DisplayMemberPath = "Name";

            cbBoxMaterialsTop.ItemsSource = materials;
            cbBoxMaterialsTop.DisplayMemberPath = "Name";

            cbBoxMaterialsMiddle.ItemsSource = materials;
            cbBoxMaterialsMiddle.DisplayMemberPath = "Name";

            cbBoxMaterialsBottom.ItemsSource = materials;
            cbBoxMaterialsBottom.DisplayMemberPath = "Name";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Level levelTop = cbBoxLevelsTop.SelectedItem as Level;
            Level levelMiddle = cbBoxLevelsMiddle.SelectedItem as Level;
            Level levelBottom = cbBoxLevelsBottom.SelectedItem as Level;

            if (levelTop.Elevation > levelMiddle.Elevation && levelMiddle.Elevation > levelBottom.Elevation)
            {
                materialAndLevels = new List<MaterialAndLevel>
                {
                    new MaterialAndLevel(levelTop,cbBoxMaterialsTop.SelectedItem as Material),
                    new MaterialAndLevel(levelMiddle,cbBoxMaterialsMiddle.SelectedItem as Material),
                    new MaterialAndLevel(levelBottom,cbBoxMaterialsBottom.SelectedItem as Material),
                };

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Nhập sai thứ tự level");
            }
        }
    }
}