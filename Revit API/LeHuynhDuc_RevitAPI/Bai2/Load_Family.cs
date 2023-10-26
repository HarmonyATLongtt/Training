using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Document = Autodesk.Revit.DB.Document;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    public class Load_Family : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            // Open a file dialog to select a family file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Family Files (*.rfa)|*.rfa|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a Family File (RFA)";

            Family family = null;
            string fileName = "";
            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;
                using (Transaction trans = new Transaction(doc, "Load_Family"))
                {
                    trans.Start();
                    if (doc.LoadFamily(fileName, out family))
                    {
                        TaskDialog.Show("Thông báo", "Load Family thành công!");
                    }
                    else
                        TaskDialog.Show("Thông báo", "Load Family không thành công!");

                    trans.Commit();
                }

            }    
            return Result.Succeeded;
        }
    }
}