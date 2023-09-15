using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreateInstanceInEachLevel.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CreateInstanceInEachLevel
{
    public partial class Form1 : System.Windows.Forms.Form
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

        public Form1(ExternalCommandData command)
        {
            uiDoc = command.Application.ActiveUIDocument;
            doc = uiDoc.Document;
            levelModels = new List<LevelModel>();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
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