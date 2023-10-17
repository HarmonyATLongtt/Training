using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace Create_ModelLine.Forms
{
    public partial class ModelLineForm : System.Windows.Forms.Form
    {
        private Document doc = null;
        private List<Level> levelModels;
        private List<View> viewModels;
        private List<Element> modelLines;

        private Level _level;

        public Level Level
        {
            set { _level = value; }
            get { return _level; }
        }

        private View _selectedView;

        public View SelectedView
        {
            get { return _selectedView; }
            set { _selectedView = value; }
        }

        private ElementId _modelLineSelect;

        public ElementId ModelLineSelect
        {
            get => _modelLineSelect;
            set => _modelLineSelect = value;
        }

        public bool Break = false;
        public bool detailCheck = false;

        public ModelLineForm(Document doc)
        {
            this.doc = doc;
            levelModels = new List<Level>();
            viewModels = new List<View>();
            modelLines = new List<Element>();

            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void ModelLineForm_Load(object sender, EventArgs e)
        {
            var lineTemp = doc.Create.NewModelCurve(Line.CreateBound(XYZ.Zero, new XYZ(0, 1, 0)),
                        SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero)))
                as ModelLine;

            foreach (var ele in lineTemp.GetLineStyleIds().OrderBy(ele => doc.GetElement(ele).Name))
            {
                modelLines.Add(doc.GetElement(ele));
            }

            cboLineStyle.DataSource = modelLines;
            cboLineStyle.DisplayMember = "Name";
            cboLineStyle.ValueMember = "Id";

            doc.Delete(lineTemp.Id);

            levelModels = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(ls => ls.Elevation)
                .ToList();

            viewModels = GetAllViews(doc);

            if (rdModelLine.Checked == true)
            {
                lbView_Level.Text = "Level:";
                cboLevel_View.DataSource = levelModels;
                cboLevel_View.DisplayMember = "Name";
            }
            else
            {
                lbView_Level.Text = "View:";
                cboLevel_View.DataSource = viewModels;
                cboLevel_View.DisplayMember = "Title";
            }
            Break = true;
        }

        private void rdModelLine_CheckedChanged(object sender, EventArgs e)
        {
            lbView_Level.Text = "Level:";
            cboLevel_View.DataSource = levelModels;
            cboLevel_View.DisplayMember = "Name";
        }

        private void rdDetailLine_CheckedChanged(object sender, EventArgs e)
        {
            lbView_Level.Text = "View:";
            cboLevel_View.DataSource = viewModels;
            cboLevel_View.DisplayMember = "Title";
        }

        private List<View> GetAllViews(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(View));
            List<View> views = new List<View>();

            foreach (Element element in collector)
            {
                View view = element as View;
                if (view != null && !view.IsTemplate && view.IsViewValidForTemplateCreation() && view is ViewPlan)
                    views.Add(view);
            }

            return views.OrderBy(v => v.Name).ToList();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rdDetailLine.Checked)
            {
                Level = null;
                detailCheck = true;
                SelectedView = viewModels[cboLevel_View.SelectedIndex];
            }
            else
            {
                SelectedView = null;
                Level = levelModels[cboLevel_View.SelectedIndex];
            }

            ModelLineSelect = cboLineStyle.SelectedValue as ElementId;

            Break = false;

            Close();
            return;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Break = true;
            Close();
            return;
        }
    }
}