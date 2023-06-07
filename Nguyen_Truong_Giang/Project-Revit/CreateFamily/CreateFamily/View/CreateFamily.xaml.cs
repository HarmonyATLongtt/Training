using Autodesk.Revit.DB;
using CreateFamily.Models;
using CreateFamily.ViewModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CreateFamily
{
    /// <summary>
    /// Interaction logic for CreateFamily.xaml
    /// </summary>
    public partial class CreateFamily : Window
    {
        public CreateFamily(Document doc)
        {
            InitializeComponent();

            SelectModel model = new SelectModel(doc);
            DataContext = new MainModelView(model);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}