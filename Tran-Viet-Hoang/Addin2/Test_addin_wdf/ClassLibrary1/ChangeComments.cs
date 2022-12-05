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

namespace ChangeComments
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
            if (element != null)
            {
                using (var transaction = new Transaction(doc, "Set Mark"))
                {
                    transaction.Start();
                    element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("No mark here");


                    if (element is FamilyInstance inst)
                    {
                        var beamType = doc.GetElement(inst.GetTypeId()) as FamilySymbol;
                        Parameter para = beamType.LookupParameter("b");
                        if (para != null
                            && !para.IsReadOnly
                            && para.StorageType == StorageType.Double)
                        {
                            //para.Set(900 / 304.8);
                            string val = (para.AsDouble()*304.8).ToString();
                            MessageBox.Show(val);
                        }
                    }              
                    transaction.Commit();
                }
            }
            else
                MessageBox.Show("nguoi dung huy thao tac");

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
