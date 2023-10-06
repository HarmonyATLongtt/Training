using Autodesk.Revit.DB;
using RevitTrainees.Models;
using System;
using System.Collections.Generic;

namespace RevitTrainees.Forms
{
    public partial class Form_FilterWall : System.Windows.Forms.Form
    {
        public Autodesk.Revit.DB.View SelectedView = null;
        private Document doc = null;
        private List<ViewModel> listViewModels;

        public Form_FilterWall(Document doc)
        {
            this.doc = doc;
            listViewModels = new List<ViewModel>();
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            SelectedView = cboView.SelectedValue as View;
            Close();
            return;
        }

        private void Form_FilterWall_Load(object sender, EventArgs e)
        {
            foreach (View v in GetAllViews(doc))
            {
                listViewModels.Add(new ViewModel() { ViewSelect = v, Name = v.ViewType + ": " + v.Name });
            }

            cboView.DataSource = listViewModels;
            cboView.DisplayMember = "Name";
            cboView.ValueMember = "ViewSelect";
        }

        private List<View> GetAllViews(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(View));
            List<View> views = new List<View>();

            foreach (Element element in collector)
            {
                View view = element as View;
                if (view != null && !view.IsTemplate && view.IsViewValidForTemplateCreation())
                    views.Add(view);
            }

            return views;
        }
    }
}