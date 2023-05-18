using Autodesk.Revit.DB;
using ExRevitAPI.Models;
using ExRevitAPI.ModelView;
using System.Windows;

namespace ExRevitAPI
{
    /// <summary>
    /// Interaction logic for Select.xaml
    /// </summary>
    public partial class SelectView : Window
    {
        public SelectView(Document doc)
        {
            InitializeComponent();

            SelectModel model = new SelectModel(doc);
            DataContext = new MainModelView(model);
        }
    }
}