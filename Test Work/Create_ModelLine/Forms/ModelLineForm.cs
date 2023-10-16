using Autodesk.Revit.DB;
using Create_ModelLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Document = Autodesk.Revit.DB.Document;

namespace Create_ModelLine.Forms
{
    public partial class ModelLineForm : System.Windows.Forms.Form
    {
        private Document doc = null;
        private List<LevelModel> levelModels;
        private List<ViewModel> viewModels;
        private List<ModelLineModel> modelLines;

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
            levelModels = new List<LevelModel>();
            viewModels = new List<ViewModel>();
            modelLines = new List<ModelLineModel>();

            InitializeComponent();
        }

        private void ModelLineForm_Load(object sender, EventArgs e)
        {
            var lineTemp = doc.Create.NewModelCurve(Line.CreateBound(XYZ.Zero, new XYZ(0, 1, 0)),
                        SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero)))
                as ModelLine;

            foreach (var ele in lineTemp.GetLineStyleIds().OrderBy(ele => doc.GetElement(ele).Name))
            {
                modelLines.Add(new ModelLineModel() { Name = doc.GetElement(ele).Name, LineStyle = ele });
            }

            cboLineStyle.DataSource = modelLines;
            cboLineStyle.DisplayMember = "Name";
            cboLineStyle.ValueMember = "LineStyle";

            doc.Delete(lineTemp.Id);

            List<Level> levels = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(ls => ls.Elevation)
                .ToList();

            foreach (Level level in levels)
            {
                levelModels.Add(new LevelModel() { Level = level, Name = level.Name });
            }

            foreach (View v in GetAllViews(doc))
            {
                viewModels.Add(new ViewModel() { ViewSelect = v, Name = v.ViewType + ": " + v.Name });
            }

            if (rdModelLine.Checked == true)
            {
                lbView_Level.Text = "Level:";
                cboLevel_View.DataSource = levelModels;
                cboLevel_View.DisplayMember = "Name";
                cboLevel_View.ValueMember = "Level";
            }
            else
            {
                lbView_Level.Text = "View:";
                cboLevel_View.DataSource = viewModels;
                cboLevel_View.DisplayMember = "Name";
                cboLevel_View.ValueMember = "ViewSelect";
            }
            Break = true;
        }

        private void rdModelLine_CheckedChanged(object sender, EventArgs e)
        {
            lbView_Level.Text = "Level:";
            cboLevel_View.DataSource = levelModels;
            cboLevel_View.DisplayMember = "Name";
            cboLevel_View.ValueMember = "Level";
        }

        private void rdDetailLine_CheckedChanged(object sender, EventArgs e)
        {
            lbView_Level.Text = "View:";
            cboLevel_View.DataSource = viewModels;
            cboLevel_View.DisplayMember = "Name";
            cboLevel_View.ValueMember = "ViewSelect";
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

            return views;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rdDetailLine.Checked)
            {
                Level = null;
                detailCheck = true;
                SelectedView = cboLevel_View.SelectedValue as View;
            }
            else
            {
                SelectedView = null;
                Level = cboLevel_View.SelectedValue as Level;
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