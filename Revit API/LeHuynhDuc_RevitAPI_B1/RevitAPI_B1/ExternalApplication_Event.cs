using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace RevitAPI_B1
{
    public class ExternalApplication_Event : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            application.DocumentChanged -= ElementChangedEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            try
            {
                application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(ElementChangedEvent);
            }
            catch (Exception e)
            {
                return ExternalDBApplicationResult.Failed;
            }
            return ExternalDBApplicationResult.Succeeded;
        }

        public void ElementChangedEvent(object sender, DocumentChangedEventArgs args)
        {
            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Furniture);
            ElementId elementId = args.GetModifiedElementIds(filter).First();
            string nameTrans = args.GetTransactionNames().First();
            //kích hoạt chỗ này
            TaskDialog.Show("Document Changed", "Phan tu da chinh sua la: " + elementId.ToString() + "\nDa bi thay doi boi transaction: " + nameTrans.ToString());
        }
        
    }
}
