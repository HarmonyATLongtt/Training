using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UIDocument uidoc { get; }
        public Document doc { get; }

        public MainWindow(UIDocument uiDoc)
        {
            uidoc = uiDoc;
            doc = uiDoc.Document;
            InitializeComponent();
        }

        private void SetViewName(object sender, RoutedEventArgs e)
        {
            using (Transaction trans = new Transaction(doc, "Set View Name"))
            {
                trans.Start();
                doc.ActiveView.Name = tbBtn.Text;

                trans.Commit();
            }
        }
    }
}