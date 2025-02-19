using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateColumn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                using (Transaction trans = new Transaction(doc, "Create Column"))
                {
                    trans.Start();
                    // Lấy FamilySymbol của cột
                    FamilySymbol columnSymbol = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_StructuralColumns)
                        .FirstOrDefault() as FamilySymbol;
                    if (columnSymbol == null)
                    {
                        message = "Không tìm thấy FamilySymbol cho cột.";
                        return Result.Failed;
                    }

                    // Kích hoạt FamilySymbol nếu chưa được kích hoạt
                    if (!columnSymbol.IsActive)
                    {
                        columnSymbol.Activate();
                        doc.Regenerate();
                    }
                    // Lấy Level (cấp độ) đầu tiên trong mô hình
                    Level level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault();
                    if (level == null)
                    {
                        message = "Không tìm thấy Level trong dự án.";
                        return Result.Failed;
                    }

                    // Tọa độ để đặt cột
                    XYZ location = new XYZ(0, 0, 0);

                    // Tạo cột
                    FamilyInstance column = doc.Create.NewFamilyInstance(location, columnSymbol, level, StructuralType.Column);
                    trans.Commit();

                    // In ra các parameter của cột
                    TaskDialog.Show("Parameters", GetColumnParameters(column));
                    // Thêm một parameter mới
                    trans.Start("Set parameter");
                    Parameter param = column.LookupParameter("Mark");
                    if (param != null && !param.IsReadOnly)
                    {
                        param.Set("Column001");
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Parameter 'Mark' không tồn tại hoặc không thể chỉnh sửa.");
                    }
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private string GetColumnParameters(FamilyInstance column)
        {
            string paraInfos = "";
            foreach (Parameter param in column.Parameters)
            {
                paraInfos += $"Name: {param.Definition.Name}, Value: {param.AsString() ?? param.AsValueString()}\n";
            }

            return paraInfos;
        }
    }
}