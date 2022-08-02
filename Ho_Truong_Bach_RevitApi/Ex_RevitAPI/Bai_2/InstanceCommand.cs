using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bai_2.View;

namespace Bai_2.ViewModel
{
    [Transaction(TransactionMode.Manual)]
    public class InstanceCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                if (r != null)
                {
                    Element element = doc.GetElement(r);
                    MainViewModel mainViewModel = new MainViewModel(element, doc);
                    mainViewModel.InstanceInfor.ShowDialog();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            
        }
    }
}
