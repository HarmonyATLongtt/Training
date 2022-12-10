using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
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

            var getcmt = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).First();
            var cmtvalue = getcmt.LookupParameter("Comments").AsString();
            if (cmtvalue != null && cmtvalue != "")
            {
                MessageBox.Show("This element comment now is: " + cmtvalue);
            }
            else
            {
                MessageBox.Show("No comments found");
            }


            using (var transaction = new Transaction(doc, "Set Mark"))
            {
                transaction.Start();             
                getcmt.LookupParameter("Comments").Set(num.ToString());              
                MessageBox.Show("Then it is: " + num);
                transaction.Commit();
            }
            return Result.Succeeded;
        }

    }
}
