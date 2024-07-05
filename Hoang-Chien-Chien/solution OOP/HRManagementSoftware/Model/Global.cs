using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace HRManagementSoftware
{
    [Serializable]
    public class Global
    {
        public static Company company = new Company();
        public static List<SubsidiaryCom> subsidiaryComs = new List<SubsidiaryCom>();
        public static List<Department> departments = new List<Department>();
        public static List<Employee> employees = new List<Employee>();
        public static int _idEmpCounter = 1;
        public static int _idDepCounter = 1;
        public static int _idSubCounter = 1;


        
        public enum ChucVu
        {
            [Description("Tổng Giám Đốc")]
            TongGiamDoc = 1,
            [Description("Giám Đốc")]
            GiamDoc = 2,
            [Description("Trưởng Phòng")]
            TruongPhong = 3,
            [Description("Phó Phòng")]
            PhoPhong = 4,
            [Description("Nhân Viên")]
            NhanVien = 5
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }
}
