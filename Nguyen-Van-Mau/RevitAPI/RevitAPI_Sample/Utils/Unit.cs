using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_Sample.Utils
{
    public struct SIUnit
    {
        /// <summary>
        /// Lengh : feet to mm
        /// </summary>
        public const double ToLength = 304.8;

        /// <summary>
        /// Area : feet2 to mm2
        /// </summary>
        public static double ToArea = Math.Pow(304.8, 2);

        /// <summary>
        /// Volume: feet3 to mm3
        /// </summary>
        public static double ToVolume = Math.Pow(304.8, 3);
    }
}