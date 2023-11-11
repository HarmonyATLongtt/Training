using System.Collections.Generic;

namespace ConcreteFacing.DATA
{
    public class OptionsSetData
    {
        public virtual List<string> imgpaths { get; set; }

        public virtual double[] imgsize { get; set; }

        public OptionsSetData()
        {
            imgpaths = new List<string>();
            // imgsize {height, width}
            //imgsize = new string[2] { "Height", "Width"};
            imgsize = new double[2] { 200, 350 };
        }
    }
}