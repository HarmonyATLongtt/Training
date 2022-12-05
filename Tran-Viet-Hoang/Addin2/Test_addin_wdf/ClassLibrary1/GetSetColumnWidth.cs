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
using ClassLibrary1;

namespace GetSetColumnWidth
{
    
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Class1 : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            
            var element = PickConcreteColumn(uidoc);

            Random rnd = new Random();
            int num = rnd.Next(300,900);
            if (element != null)
            {
                using (var transaction = new Transaction(doc, "Get Set Column Width"))
                {
                    transaction.Start();
                    if (element is FamilyInstance inst)
                    {
                        var beamType = doc.GetElement(inst.GetTypeId()) as FamilySymbol;
                        Parameter para = beamType.LookupParameter("b");
                        if (para != null
                            && !para.IsReadOnly
                            && para.StorageType == StorageType.Double)
                        {
                            string val = (para.AsDouble()*304.8).ToString();                         
                            MessageBox.Show("Giá trị b hiện tại của cột " +beamType.Name.ToString()+" là: "+val+" mm");
                            para.Set(num / 304.8);
                            MessageBox.Show("Giá trị b của cột " + beamType.Name.ToString() + " trở thành là: " + num + " mm");
                        }
                    }              
                    transaction.Commit();
                }
            }
            else
                MessageBox.Show("Người dùng hủy thao tác");

            return Result.Succeeded;
        }

        private Element PickConcreteColumn(UIDocument uidoc)
        {
            try
            {
                Reference refer = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new AllowColumn(), "chon cot be tong");
                if (refer != null)
                    return uidoc.Document.GetElement(refer);
            }
            catch (OperationCanceledException) { }
            return null;
        }
    }
}
