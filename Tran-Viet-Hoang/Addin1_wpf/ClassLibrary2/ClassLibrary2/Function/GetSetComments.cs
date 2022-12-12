using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;



namespace GetSetComments
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AppRevit : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Random rnd = new Random();
            int num = rnd.Next(100);

            var selectelem = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).First();
            var elemcmtvalue = selectelem.LookupParameter("Comments").AsString();
            if (elemcmtvalue != null && elemcmtvalue != "")
            {
                MessageBox.Show("This element comment now is: " + elemcmtvalue);
            }
            else
            {
                MessageBox.Show("No comments found");
            }


            using (var transaction = new Transaction(doc, "Set Mark"))
            {
                transaction.Start();             
                selectelem.LookupParameter("Comments").Set(num.ToString());              
                MessageBox.Show("Then it is: " + num);
                transaction.Commit();
            }
            return Result.Succeeded;
        }

    }
}
