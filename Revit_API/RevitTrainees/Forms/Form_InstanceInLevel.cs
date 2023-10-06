using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTrainees.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RevitTrainees.Forms
{
    public partial class Form_InstanceInLevel : System.Windows.Forms.Form
    {
        private List<LevelModel> levelModels;
        private UIDocument uiDoc = null;
        private Document doc = null;
        private Level _level;

        public Level Level
        {
            set { _level = value; }
            get { return _level; }
        }

        public Form_InstanceInLevel(ExternalCommandData command)
        {
            uiDoc = command.Application.ActiveUIDocument;
            doc = uiDoc.Document;
            levelModels = new List<LevelModel>();
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void Form_InstanceInLevel_Load(object sender, EventArgs e)
        {
            List<Level> levels = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();
            foreach (Level level in levels)
            {
                levelModels.Add(new LevelModel() { Level = level, Name = level.Name });
            }
            cboLevel.DataSource = levelModels;
            cboLevel.DisplayMember = "Name";
            cboLevel.ValueMember = "Level";
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            Level = cboLevel.SelectedValue as Level;
            Close();
            return;
        }
    }
}