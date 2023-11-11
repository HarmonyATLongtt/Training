using System.Collections.Generic;

namespace ConcreteFacing.DATA
{
    public class OptionsSetColumnData : OptionsSetData
    {
        public override List<string> imgpaths { get; set; }

        public OptionsSetColumnData()
        {
            double imgheight = 350;
            double imgwidth = 300;
            imgpaths = new List<string>() {
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colfront.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colback.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colleft.png",
                "G:/01Thuctap/Training/Tran-Viet-Hoang/Addin1_wpf/ClassLibrary2/ConcreteFacing/Facelayout/Colright.png",
            };

            imgsize = new double[2] { imgheight, imgwidth };
        }
    }
}