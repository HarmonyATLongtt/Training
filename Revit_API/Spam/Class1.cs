using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace Spam
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentChanged +=
                new System.EventHandler<Autodesk.Revit.DB.Events.DocumentChangedEventArgs>(DocumentChangedEvent);
            return Result.Succeeded;
        }

        private void DocumentChangedEvent(object sender, DocumentChangedEventArgs e)
        {
            ICollection<ElementId> oElementAdded = e.GetAddedElementIds();
            ICollection<ElementId> oElementDeleted = e.GetDeletedElementIds();

            if (oElementAdded.Count > 0)
                TaskDialog.Show("A", "Add: " + oElementAdded.Count.ToString());
            if (oElementDeleted.Count > 0)
                TaskDialog.Show("A", "Delete: " + oElementDeleted.Count.ToString());
        }
    }
}