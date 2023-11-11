using System.Collections.Generic;

namespace ConcreteFacing.DATA
{
    public class OptionsSetBeamData : OptionsSetData
    {
        public override List<string> imgpaths { get; set; }

        public OptionsSetBeamData()
        {
            double imgheight = 300;
            double imgwidth = 400;
            imgpaths = new List<string>() {
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Beamtop.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Beambottom.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Beamfront.png",
                    "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Beamback.png",
                    };

            imgsize = new double[2] { imgheight, imgwidth };
        }
    }
}