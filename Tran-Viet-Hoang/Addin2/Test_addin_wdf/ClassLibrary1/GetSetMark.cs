using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using ClassLibrary1.UI.ViewModel;
using ClassLibrary1.UI.Views;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows;
using System.Xml.Linq;

namespace GetSetMark
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
           UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Reference pickObject = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Element element = doc.GetElement(pickObject);
            if (element != null)
            {
                using (var transaction = new Transaction(doc, "Get Set Column Width"))
                {
                    transaction.Start();
                    string ppp = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString().ToString();
                    double? set = element.get_Parameter(BuiltInParameter.COLUMN_BASE_ATTACHMENT_OFFSET_PARAM).AsDouble()/304.8 ;
                    //string mark = element.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString();
                    MessageBox.Show("Bạn đã chọn Elenment có category là " + set.ToString());
                    element.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).Set("Cột tự tạo");
                    MessageBox.Show("Bạn đã chọn Elenment có category trở thành là: Cột tự tạo ");
                    transaction.Commit();
                }
            }
            else
            {
                MessageBox.Show("Người dùng hủy thao tác");
            }
          
            return Result.Succeeded;
        }
    }
}
