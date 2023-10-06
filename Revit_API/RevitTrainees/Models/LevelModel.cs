using Autodesk.Revit.DB;

namespace RevitTrainees.Models
{
    public class LevelModel
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private Level _level;

        public Level Level
        {
            get { return _level; }
            set { _level = value; }
        }
    }
}