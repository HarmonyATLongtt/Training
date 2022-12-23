using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Data
{
    public class RebarSetData
    {
        public double Type { get; set; } //diameter
        public int Number { get; set; }
        public double   Spacing { get; set; }
        public double RebarCrossSectionArea { get; set; } //tổng diện tích mặt cắt thép

        public double CrossSectionWidth { get; set; } // bề rộng mặt cắt rải thép

    }
}
