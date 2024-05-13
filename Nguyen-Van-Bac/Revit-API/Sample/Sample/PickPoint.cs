using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class PickPoint : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region Init

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            #endregion
            try
            {
                XYZ point1 = uidoc.Selection.PickPoint();
                XYZ point2 = uidoc.Selection.PickPoint();
                XYZ point3 = uidoc.Selection.PickPoint();
                XYZ point4 = uidoc.Selection.PickPoint();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(point1.ToString());
                sb.AppendLine(point2.ToString());
                sb.AppendLine(point3.ToString());
                sb.AppendLine(point4.ToString());
                MessageBox.Show("You have selected points:\r\n" + sb.ToString());
            }
			catch(Autodesk.Revit.Exceptions.OperationCanceledException e)
			{
                MessageBox.Show($"You Has Press Esc\n{e}", "Warning", MessageBoxButtons.OK);
                return Result.Cancelled;
			}
            return Result.Succeeded;
        }
    }
}
