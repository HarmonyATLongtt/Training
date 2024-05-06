using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace ExRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class ChangeParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                ElementId elementID = r.ElementId;
                Element element = doc.GetElement(elementID);

                if (r != null)
                {
                    Parameter para = element.LookupParameter("H");

                    using (Transaction trans = new Transaction(doc, "set para B"))
                    {
                        trans.Start();
                        para.Set(15);
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Result.Succeeded;
        }
    }
}