using Autodesk.Revit.DB;
using ClassLibrary2.Data.FrameData;

namespace ClassLibrary2.Data
{
    public class ConcreteBeamData : AllFrameData
    {

     
        //bảng Concrete Beam Flexure Envelope
        public double AsTopLongitudinal { get; set; }
        public double AsBottomLongitudinal { get; set; }


    }
}