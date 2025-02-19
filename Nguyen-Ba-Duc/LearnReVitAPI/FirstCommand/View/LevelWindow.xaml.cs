using System;
using System.Collections.Generic;
using System.Linq;

using System.Windows;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using ComboBox = System.Windows.Controls.ComboBox;
using System.Collections.ObjectModel;
using Autodesk.Revit.DB.Structure;

namespace FirstCommand.View
{
    public partial class LevelWindow : Window
    {
        public Level SelectedLevel { get; set; }
        private Document doc { get; set; }

        public LevelWindow(Document doc)
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
                .ToList();

            comboBoxLevels.ItemsSource = levels;
            comboBoxLevels.DisplayMemberPath = "Name";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedLevel = comboBoxLevels.SelectedItem as Level;
            this.DialogResult = true;
            this.Close();
        }
    }
}