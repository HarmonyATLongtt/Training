using ClassLibrary2.Data.FrameData;

namespace ClassLibrary2.Data
{
    public class ConcreteBeamData : ConcreteHostData
    {
        //bảng Concrete Beam Flexure Envelope
        public double AsTopLongitudinal { get; set; }

        public double AsBottomLongitudinal { get; set; }

        public string AsName { get; set; }
    }
}