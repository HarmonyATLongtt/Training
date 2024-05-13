using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
        [Transaction(TransactionMode.ReadOnly)]
        public class InstanceInfoCommand : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;
                Document doc = uiDoc.Document;

                // Tạo một danh sách để lưu trữ thông tin của các instance
                List<string> instanceInfoList = new List<string>();

                // Lọc tất cả các instance trong dự án
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ICollection<Element> instances = collector.OfClass(typeof(FamilyInstance)).ToElements();

                // Đếm số lượng của mỗi Category, Family, Type và Instance
                Dictionary<string, int> categoryCount = new Dictionary<string, int>();
                Dictionary<string, int> familyCount = new Dictionary<string, int>();
                Dictionary<string, int> typeCount = new Dictionary<string, int>();
                int instanceCount = 0;

                foreach (Element element in instances)
                {
                    // Lấy thông tin Category, Family và Type của instance
                    Category category = element.Category;
                    FamilySymbol familySymbol = (element as FamilyInstance).Symbol;
                    Family family = familySymbol.Family;
                    string typeName = familySymbol.Name;

                    // Tăng số lượng của từng Category, Family, Type và Instance
                    categoryCount[category.Name] = categoryCount.ContainsKey(category.Name) ? categoryCount[category.Name] + 1 : 1;
                    familyCount[family.Name] = familyCount.ContainsKey(family.Name) ? familyCount[family.Name] + 1 : 1;
                    typeCount[typeName] = typeCount.ContainsKey(typeName) ? typeCount[typeName] + 1 : 1;
                    instanceCount++;
                }

                // Hiển thị thông tin
                instanceInfoList.Add($"Số lượng Category: {categoryCount.Count}");
                foreach (var kvp in categoryCount)
                {
                    instanceInfoList.Add($"{kvp.Key}: {kvp.Value}");
                }

                instanceInfoList.Add($"Số lượng Family: {familyCount.Count}");
                foreach (var kvp in familyCount)
                {
                    instanceInfoList.Add($"{kvp.Key}: {kvp.Value}");
                }
                instanceInfoList.Add($"Số lượng Type: {typeCount.Count}");
                foreach (var kvp in typeCount)
                {
                    instanceInfoList.Add($"{kvp.Key}: {kvp.Value}");
                }

                instanceInfoList.Add($"Số lượng Instance: {instanceCount}");

                // Hiển thị thông tin ra màn hình
                TaskDialog dialog = new TaskDialog("Instance Information");
                dialog.MainContent = string.Join("\n", instanceInfoList);
                dialog.Show();

                return Result.Succeeded;
            }
        }
}

