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
using Autodesk.Revit.UI;
using ClassLibrary1.Model;
using ClassLibrary1.ModelVieww;

namespace ClassLibrary1.NewFolder1
{
    /// <summary>
    /// Interaction logic for LoadLevel.xaml
    /// </summary>
    public partial class LoadLevel : Window
    {
        Document Doc;

        public LoadLevel(Document doc)
        {
            InitializeComponent();

            SelectModel model = new SelectModel(doc);

            DataContext = new MainModelView(model);
        }
    }
}
