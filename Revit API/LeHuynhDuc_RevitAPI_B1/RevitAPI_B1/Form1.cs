using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Color = Autodesk.Revit.DB.Color;
using Form = System.Windows.Forms.Form;
using View = Autodesk.Revit.DB.View;

namespace RevitAPI_B1
{
    public partial class Form1 : Form
    {
        private UIApplication uiApp;
        private UIDocument uiDoc;
        private Autodesk.Revit.ApplicationServices.Application app;
        private Document doc;

        public Form1(ExternalCommandData commandData)
        {
            InitializeComponent();
            uiApp = commandData.Application;
            uiDoc = uiApp.ActiveUIDocument;
            doc = uiDoc.Document;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IList<Level> levels = new FilteredElementCollector(doc)
                                    .OfClass(typeof(Level))
                                    .Cast<Level>()
                                    .ToList();
            foreach (Level level in levels)
            {
                cboLevels.Items.Add(level);
            }
            cboLevels.DisplayMember = "Name";
            cboLevels.SelectedIndex = 0;

            IList<View> views = new FilteredElementCollector(doc)
                                .OfClass(typeof(View))
                                .Cast<View>().
                                ToList();

            foreach (View view in views)
            {
                cboViews.Items.Add(view);

            }
            cboViews.DisplayMember = "Name";

            cboViews.SelectedIndex = 0;

        }

        private void btnLevels_Click(object sender, EventArgs e)
        {
            Level level = cboLevels.SelectedItem as Level; 
            XYZ p1 = new XYZ(0, 0, 0);
            XYZ p2 = new XYZ(10, 0, 0);
            Line line = Line.CreateBound(p1, p2);
            using(Transaction trans = new Transaction(doc, "Create instance"))
            {
                trans.Start();
                Wall wall = Wall.Create(doc, line, level.Id, false);
                if (wall != null)
                    MessageBox.Show("Da tao thanh cong!", "Thong bao"); 
                else
                    MessageBox.Show("That bai!", "Thong bao");
                trans.Commit();
            }
            
        }

        private void btnViews_Click(object sender, EventArgs e)
        {
            // create list of categories that will for the filter
            List<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(BuiltInCategory.OST_Doors));

            // create a list of rules for the filter
            IList<FilterRule> filterRules = new List<FilterRule>();
            View currentView = cboViews.SelectedItem as View;
            ParameterFilterElement parameterFilterElemen = null;
            using (Transaction trans = new Transaction(doc, "fillter_Doors"))
            {
                trans.Start();
                try
                {
                    //parameterFilterElemen = ParameterFilterElement.Create(doc, "Filter Door", categories);
                    parameterFilterElemen = ParameterFilterElement.Create(doc, "Filter_Door" + (new Random()).Next(0, 10).ToString(), categories);
                    currentView.AddFilter(parameterFilterElemen.Id);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Message: ", ex.Message);
                }
                trans.Commit();
            }
            OverrideGraphicSettings overr = new OverrideGraphicSettings();
            overr.SetProjectionLineColor(new Color(255, 255, 0));
            using (Transaction trans = new Transaction(doc, "OverrideGraphic"))
            {
                trans.Start();
                currentView.SetFilterOverrides(parameterFilterElemen.Id, overr);
                trans.Commit();
            }

        }
    }
}
