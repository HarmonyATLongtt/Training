using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstCommand.Support.TransactionHandle;

namespace FirstCommand.Support.DebugTest
{
    public static class TestForDebug
    {
        /// <summary>
        /// Hiển thị danh sách các giá trị theo từng dòng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void ShowValues<T>(List<T> list)
        {
            string str = "";
            foreach (var l in list)
            {
                str += l.ToString();
                str += "\n";
            }
            TaskDialog.Show("Nofi", str);
        }

        /// <summary>
        /// Hiển thị danh sách các điểm thành 1 string có ngăn cách bởi dấu chấm phẩy
        /// </summary>
        /// <param name="points"></param>
        public static void ShowPointsToDraw(List<XYZ> points)
        {
            string str = "";
            foreach (var p in points)
            {
                str += p.ToString();
                str += ";";
            }
            TaskDialog.Show("Nofi", str);
        }

        /// <summary>
        /// Tạo Directshape từ solid
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid"></param>
        public static void CreateDirectShapeFromSolid(Document doc, Solid solid)
        {
            HandleForTransaction.RunTransaction(doc, "Create DirectShape", (Transaction t) =>
            {
                DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.SetShape(new List<GeometryObject> { solid });
            });
        }
    }
}