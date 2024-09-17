using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace LoadLevel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private List<Level> _levels;

        public MainWindow(UIDocument uidoc)
        {
            InitializeComponent();
            _uidoc = uidoc;
            _doc = uidoc.Document;
            LoadLevels();
        }

        private void LoadLevels()
        {
            // Lấy tất cả các levels trong project
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            _levels = collector.OfClass(typeof(Level)).Cast<Level>().ToList();

            // Load các levels vào ComboBox
            foreach (Level level in _levels)
            {
                LevelComboBox.Items.Add(level.Name);
            }

            if (LevelComboBox.Items.Count > 0)
            {
                LevelComboBox.SelectedIndex = 0;
            }
        }

        private void CreateInstanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (LevelComboBox.SelectedItem == null) return;

            string selectedLevelName = LevelComboBox.SelectedItem.ToString();
            Level selectedLevel = _levels.FirstOrDefault(l => l.Name == selectedLevelName);

            if (selectedLevel == null) return;

            using (Transaction tx = new Transaction(_doc, "Create Instance"))
            {
                tx.Start();

                // Tạo một instance tại level đã chọn, ví dụ một wall đơn giản
                XYZ start = new XYZ(0, 0, 0);
                XYZ end = new XYZ(10, 0, 0);
                Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(start, end);

                Wall.Create(_doc, line, selectedLevel.Id, false);

                tx.Commit();
            }
        }
    }
}