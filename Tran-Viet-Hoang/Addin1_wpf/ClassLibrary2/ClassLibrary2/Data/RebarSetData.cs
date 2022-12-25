using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Data
{
    public class RebarSetData
    {
        public double Type { get; set; } //diameter : đường kính
        public int Number { get; set; } // số thanh
        public double Spacing { get; set; } //khoảng cách giữa các thanh
        public double RebarCrossSectionArea { get; set; } //tổng diện tích mặt cắt thép
        public double CrossSectionWidth { get; set; } // bề rộng mặt cắt rải thép
        public ElementId HostID { get; set;}

    }
}
