using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace Ex_RevitAPI.Bai_1.ViewModel
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class InstanceCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) //ExternalCommandData truy cập dữ liệu RV
        {
            try
            {
                //Get UIDocument
                UIDocument uidoc = commandData.Application.ActiveUIDocument;

                //Get Document
                Document doc = uidoc.Document;

                //
                MainViewModel mainview = new MainViewModel(doc);
                mainview.FilterView.ShowDialog();

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
