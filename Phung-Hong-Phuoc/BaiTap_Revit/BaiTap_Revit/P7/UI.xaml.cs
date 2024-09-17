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
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Color = Autodesk.Revit.DB.Color;

namespace BaiTap_Revit.P7
{
    /// <summary>
    /// Interaction logic for UI.xaml
    /// </summary>
    public partial class UI : Window
    {
        //private readonly UIDocument _uidoc;
        private readonly Document Doc;

        private List<Level> _levels;
        private List<View> _views;

        public UI(Document doc)
        {
            InitializeComponent();
            Doc = doc;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Level
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
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

            //Load View
            FilteredElementCollector collector_view = new FilteredElementCollector(Doc);
            _views = collector_view.OfClass(typeof(View)).Cast<View>().ToList();

            // Load các levels vào ComboBox
            foreach (View v in _views)
            {
                ViewComboBox.Items.Add(v.Name);
            }

            if (ViewComboBox.Items.Count > 0)
            {
                ViewComboBox.SelectedIndex = 0;
            }
        }

        private void CreateInstanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (LevelComboBox.SelectedItem == null) return;

            string selectedLevelName = LevelComboBox.SelectedItem.ToString();
            Level selectedLevel = _levels.FirstOrDefault(l => l.Name == selectedLevelName);

            if (selectedLevel == null) return;

            using (Transaction tx = new Transaction(Doc, "Create Instance"))
            {
                tx.Start();

                // Tạo một instance tại level đã chọn, ví dụ một wall đơn giản
                XYZ start = new XYZ(0, 0, 0);
                XYZ end = new XYZ(10, 0, 0);
                Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(start, end);

                Wall.Create(Doc, line, selectedLevel.Id, false);

                tx.Commit();
            }

            Close();
        }

        private void CreateFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewComboBox.SelectedItem == null) return;

            string selectedViewName = ViewComboBox.SelectedItem.ToString();
            View selectedView = _views.FirstOrDefault(v => v.Name == selectedViewName);

            if (selectedView == null) return;

            using (Transaction tx = new Transaction(Doc, "Create and apply Filter for All Walls"))
            {
                try
                {
                    tx.Start();

                    // create list and categories (chỉ áp dụng cho Walls)
                    List<ElementId> cats = new List<ElementId>();
                    cats.Add(new ElementId(BuiltInCategory.OST_Walls));

                    // create empty parameter filter (no rules)
                    ParameterFilterElement paramFilter = ParameterFilterElement
                        .Create(Doc, "All Walls Filter", cats);

                    // Set graphic overrides
                    OverrideGraphicSettings overrides = new OverrideGraphicSettings();
                    overrides.SetCutLineColor(new Color(0, 255, 0)); // Màu đường cắt là xanh lá cây
                    overrides.SetCutLineWeight(3); // Độ dày đường cắt
                    overrides.SetSurfaceTransparency(20); // Độ trong suốt 20%

                    // Apply filter to the selected view
                    selectedView.AddFilter(paramFilter.Id);
                    selectedView.SetFilterVisibility(paramFilter.Id, true);
                    selectedView.SetFilterOverrides(paramFilter.Id, overrides);

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    tx.RollBack();
                }
            }

            Close();
        }
    }
}