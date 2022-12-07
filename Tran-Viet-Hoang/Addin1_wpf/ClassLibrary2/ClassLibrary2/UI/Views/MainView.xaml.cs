using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Windows;

namespace ClassLibrary2.UI.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public object ConfigurationManager { get; private set; }

        public MainView()
        {
            List<BeamSystem> GetBeamSystems_Class = new List<BeamSystem>();
            InitializeComponent();
        }
    }
}