using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Linq;

namespace FirstCommand.View
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    ///

    public partial class InputParamView : Window
    {
        private Document _doc { get; set; }

        //public string Length { get; set; }
        //public string Width { get; set; }
        //public string Height { get; set; }
        public Material Material { get; set; }

        public Level Level { get; set; }

        public List<ParamData> ListParamDatas { get; set; }

        public InputParamView(Document doc)
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

            cbBoxLevels.ItemsSource = levels;
            cbBoxLevels.DisplayMemberPath = "Name";

            var materials = new FilteredElementCollector(_doc)
                .OfClass(typeof(Material))
                .OfType<Material>()
                .OrderBy(m => m.Name)
                .ToList();

            cbBoxMaterials.ItemsSource = materials;
            cbBoxMaterials.DisplayMemberPath = "Name";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Length = txbLength.Text;
            //Width = txbWidth.Text;
            //Height = txbHeight.Text;
            Level = cbBoxLevels.SelectedItem as Level;
            Material = cbBoxMaterials.SelectedItem as Material;

            ListParamDatas = new List<ParamData>
            {
                new ParamData(lbLength.Content.ToString(),txbLength.Text),
                new ParamData(lbWidth.Content.ToString(),txbWidth.Text),
                new ParamData(lbHeight.Content.ToString(),txbHeight.Text),
                new ParamData(lbMaterial.Content.ToString(),cbBoxMaterials.SelectedItem as Material),
                new ParamData(lbLevel.Content.ToString(),cbBoxLevels.SelectedItem as Level),
            };

            this.DialogResult = true;
            this.Close();
        }
    }
}