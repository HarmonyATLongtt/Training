using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace RevitTrainees.Utils
{
    public class LoadFamily
    {
        public Family Load(Document doc)
        {
            string familyPath = "";
            Family family = null;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Family | *.rfa";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                familyPath = dialog.FileName;
            }

            if (!doc.LoadFamily(familyPath, out family))
            {
                throw new System.Exception("Unable to load " + familyPath);
            }

            return family;
        }
    }
}