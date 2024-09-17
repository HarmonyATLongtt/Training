using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms; // Thêm thư viện này để sử dụng InputBox

namespace BaiTap_Revit.P1
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class SetParameter : IExternalCommand
    {
        public class ParameterEle
        {
            public string Name { get; set; }
            public double ValuePara { get; set; }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select an element to modify parameters");

                if (reference != null)
                {
                    Element element = doc.GetElement(reference);

                    List<Parameter> editableParameters = new List<Parameter>();

                    foreach (Parameter param in element.Parameters)
                    {
                        if (param.StorageType == StorageType.Double && !param.IsReadOnly && !param.CanBeAssociatedWithGlobalParameters())
                        {
                            editableParameters.Add(param);
                        }
                    }

                    foreach (Parameter param in editableParameters)
                    {
                        double currentValue = 0;
                        try
                        {
                            // Convert parameter value from internal units to display units
                            currentValue = UnitUtils.ConvertFromInternalUnits(param.AsDouble(), param.GetUnitTypeId());
                        }
                        catch
                        {
                            currentValue = param.AsDouble();
                        }

                        // Hiển thị hộp thoại thông báo giá trị hiện tại
                        TaskDialog.Show("Edit Parameter", $"Parameter: {param.Definition.Name}\nCurrent value: {currentValue} mm");

                        // Sử dụng InputBox để yêu cầu người dùng nhập giá trị mới
                        string userInput = Microsoft.VisualBasic.Interaction.InputBox($"Enter new value for {param.Definition.Name}:", "Edit Parameter", currentValue.ToString());

                        // Kiểm tra đầu vào
                        if (double.TryParse(userInput, out double newValue))
                        {
                            // Bắt đầu transaction để ghi giá trị mới
                            using (Transaction trans = new Transaction(doc, "Set Parameter Value"))
                            {
                                trans.Start();

                                param.Set(UnitUtils.ConvertToInternalUnits(newValue, param.GetUnitTypeId()));

                                trans.Commit();
                            }
                        }
                        else
                        {
                            TaskDialog.Show("Error", "Invalid input, skipping this parameter.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }
}