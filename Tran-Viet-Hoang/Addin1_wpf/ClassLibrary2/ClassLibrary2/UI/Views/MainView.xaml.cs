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
using System.IO;
using Microsoft.Win32;
using System.Data;
using System.Data.OleDb;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.UI.Selection;


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

        private void btnload_Click(object sender, RoutedEventArgs e)
        {
            string AccessPath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Access files (*.mdb)|*.mdb";
            if (openFileDialog.ShowDialog() == true)
                txtinput.Text = openFileDialog.FileName;
                AccessPath = openFileDialog.FileName;
            //txtinput.Text = File.ReadAllText(openFileDialog.FileName);

            //Test voi file chi co 1 sheet

            OneWorkSheetTest test = new OneWorkSheetTest();
            string[] TestArray;

            DataTable dt = new DataTable();

            dt.Columns.Add("Last Name", typeof(string));
            dt.Columns.Add("First Name", typeof(string));
            dt.Columns.Add("Class", typeof(string));

            using (StreamReader sr = new StreamReader(openFileDialog.FileName))
            {
                while (!sr.EndOfStream)
                {
                    TestArray = sr.ReadLine().Split(';');
                    test.LastName = TestArray[0];
                    test.FirstName = TestArray[1];
                    test.Class = TestArray[2];
                    dt.Rows.Add(TestArray);
                }
                DataView dv = new DataView(dt);
                dgrsheet.ItemsSource = dv;

            }



        }

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
        }

        private void dgrsheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btncreate_Click(object sender, RoutedEventArgs e)
        {
          
   

            //            // get the active view's level for beam creation
            //            Level level = document.ActiveView.Level;
            //            // load a family symbol from file
            //            FamilySymbol gotSymbol = null;
            //            String fileName = @"C:\Documents and Settings\All Users\Application Data\Autodesk\RST 2011\Imperial 
            //Library\Structural\Framing\Steel\W-Wide Flange.rfa";
            //            String name = "W10X54";
            //            FamilyInstance instance = null;
            //            if (document.LoadFamilySymbol(fileName, name, out gotSymbol))
            //            {
            //                // look for a model line in the list of selected elements
            //                UIDocument uidoc = new UIDocument(document);
            //                ElementSet sel = uidoc.Selection.Elements;
            //                ModelLine modelLine = null;
            //                foreach (Autodesk.Revit.DB.Element elem in sel)
            //                {
            //                    if (elem is ModelLine)
            //                    {
            //                        modelLine = elem as ModelLine;
            //                        break;
            //                    }
            //                }
            //                if (null != modelLine)
            //                {
            //                    // create a new beam
            //                    instance = document.Create.NewFamilyInstance(modelLine.GeometryCurve, gotSymbol, level, StructuralType.Beam);
            //                }
            //                else
            //                {
            //                    throw new Exception("Please select a model line before invoking this command");
            //                }
            //            }
            //            else
            //            {
            //                throw new Exception("Couldn't load " + fileName);
            //            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //dgrsheet.ItemsSource = GetAccessData().DefaultView;
        }
        //private DataTable GetAccessData()
        //{
        //    DataTable dt = new DataTable();
        //    string connString = ConfigurationManager.ConnectionStrings["dbx"].ConnectionString;
        //    using (OleDbConnection con  = new OleDbConnection(connString))
        //    {
        //        using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM Employees", con))
        //        {
        //            con.Open();
        //            OleDbDataReader reader = cmd.ExecuteReader();
        //            dt.Load(reader);
        //        }
        //    }
        //    return dt;

        //}
    }
}
