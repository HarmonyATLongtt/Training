using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstCommand.Support.Constants
{
    public static class CommonConstants
    {
        public const double TOLERANCE = 1e-6;
        public const double COSINE_ANGLE_TOLERANCE_1_DEGREE = 0.01745; // 1 độ
        public const double COSINE_ANGLE_TOLERANCE_5_DEGREE = 0.0872; // 5 độ (góc lệch cho phép để vector normal và trục Z được coi là vuông góc)
    }
}