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
using System.IO;
using Microsoft.Win32;
using System.Data;
using System.Data.OleDb;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using Application = Autodesk.Revit.ApplicationServices.Application;



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

        //private void btnload_Click(object sender, RoutedEventArgs e)
        //{
           
        //}

        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
        }

        private void dgrsheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btncreate_Click(object sender, RoutedEventArgs e)
        {
          
 

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //dgrsheet.ItemsSource = GetAccessData().DefaultView;
        }

        private void btnload_Click_1(object sender, RoutedEventArgs e)
        {        
            //OpenFileDialog openFileDialog = new OpenFileDialog();         
            //if (openFileDialog.ShowDialog() == true)
            //    txtinput.Text = openFileDialog.FileName;
        }
       
    }
}
