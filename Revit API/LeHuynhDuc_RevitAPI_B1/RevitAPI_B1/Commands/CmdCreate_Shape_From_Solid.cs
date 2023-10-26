using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document = Autodesk.Revit.DB.Document;

namespace RevitAPI_B1.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreate_Shape_From_Solid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Sử dụng Selection để cho phép người dùng chọn một đối tượng trong Revit
            Selection selection = uiDoc.Selection;

            // Yêu cầu người dùng chọn đối tượng Instance
            IList<Reference> references = uiDoc.Selection.PickObjects(ObjectType.Element);
            List<GeometryObject> geometryObjects = new List<GeometryObject>();
            Solid result = null;
            foreach (Reference reference in references)
            {
                if (reference != null)
                {
                    ElementId elementid = reference.ElementId;
                    Element element = doc.GetElement(elementid);
                    Options opt = new Options();
                    opt.DetailLevel = ViewDetailLevel.Fine;
                    GeometryElement geometryElement = element.get_Geometry(opt);
                    //Solid solid = null;
                    if (geometryElement != null)
                    {
                        
                        foreach (GeometryObject geometryObject in geometryElement)
                        {
                            if (geometryObject is Solid instanceSolid)
                            {
                                geometryObjects.Add(instanceSolid);
                                if (result == null)
                                    result = instanceSolid;
                                else
                                {
                                    result = BooleanOperationsUtils.ExecuteBooleanOperation(result, instanceSolid, BooleanOperationsType.Union);
                                }
                                break;
                            }
                            
                        }
                    }
                }
            }

            // Kiểm tra xem đã lấy được Solid hay không
            if (geometryObjects != null)
            {

                TaskDialog.Show("Revit", geometryObjects.Count.ToString());
                using (var transaction = new Transaction(doc, "Create Shape"))
                {
                    transaction.Start();
                    // Tạo một DirectShape mới
                   // Create_DirectShape(doc, geometryObjects);
                    Create_DirectShape(doc, new List<GeometryObject> { result});
                    transaction.Commit();
                }
            }
            else
            {
                TaskDialog.Show("Revit", "Loi");
            }
            return Result.Succeeded;
        }
        public void Create_DirectShape(Document doc, List<GeometryObject> list)
        {
            DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            directShape.SetShape(list);
            Location location = directShape.Location;
            location.Move(new XYZ(10, 10, 0));
        }
    }
}