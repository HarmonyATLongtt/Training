using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Linq;

namespace Spam
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        private int oldCount = 0;
        private int newCount = 0;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            using (var trans = new Transaction(doc, "Text Note Creation"))
            {
                uiApp.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(idleUpdate);

                oldCount = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .ToList()
                .Count();

                trans.Commit();
            }

            return Result.Succeeded;
        }

        private void idleUpdate(object sender, IdlingEventArgs e)
        {
            UIApplication uiApp = sender as UIApplication;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Create a filtered element collector to find deleted elements
            newCount = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WhereElementIsViewIndependent()
                .ToList()
                .Count();

            // Check if there are any deleted elements
            if (oldCount > newCount)
            {
                TaskDialog.Show("Element Deletion Notification", $"{oldCount - newCount} elements were deleted.");
                oldCount = newCount;
            }
            else if (newCount > oldCount)
            {
                TaskDialog.Show("Element Deletion Notification", $"{-oldCount + newCount} elements were created.");
                oldCount = newCount;
            }
        }
    }
}