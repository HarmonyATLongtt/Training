using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ClassLibrary2.Data
{
    public class RebarLayoutData
    {
        public double MinSpacing { get; set; }  
        public int Number { get; set; } // số thanh
        public double Spacing { get; set; } //khoảng cách giữa các thanh
        public double RebarCrossSectionArea { get; set; } //tổng diện tích mặt cắt thép
        public double CrossSectionWidth { get; set; } // bề rộng mặt cắt rải thép
     
        public RebarLayoutData()
        {
            MinSpacing = 25 / 304.8;
        }
    }

   
}