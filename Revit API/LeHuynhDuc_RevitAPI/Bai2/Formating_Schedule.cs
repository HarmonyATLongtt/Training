using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    internal class Formating_Schedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            //lấy ra ViewShedule cần format
            ViewSchedule viewSchedule = new FilteredElementCollector(doc)
                                        .OfCategory(BuiltInCategory.OST_Schedules)
                                        .First(x => x.Name == "Schedule1") as ViewSchedule;
            ViewSchedule view1 = doc.ActiveView as ViewSchedule;
            using (Transaction trans = new Transaction(doc, "Format"))
            {
                trans.Start();
                FormatLengthFields(view1);
                trans.Commit();
            }
            return Result.Succeeded;
        }
        public void FormatLengthFields(ViewSchedule schedule)
        {
            //Lấy số lượng Field có trong Schedule
            int nFields = schedule.Definition.GetFieldCount();
            //Lặp qua tất cả các trường
            for (int n = 0; n < nFields; n++)
            {
                //Lấy Field thứ n trong Schedule
                ScheduleField field = schedule.Definition.GetField(n);
                //Kiểm tra xem Field có phải kiểu độ dài không
                if (field.GetSpecTypeId() == SpecTypeId.Length)
                {
                    //Khởi tạo formatOption mới
                    FormatOptions formatOpts = new FormatOptions();
                    formatOpts.UseDefault = false;
                    //Thiết lập đơn vị
                    formatOpts.SetUnitTypeId(UnitTypeId.MetersCentimeters);
                    //Gán định dạng cho field
                    field.SetFormatOptions(formatOpts);
                }
            }
        }
        
    }
}
