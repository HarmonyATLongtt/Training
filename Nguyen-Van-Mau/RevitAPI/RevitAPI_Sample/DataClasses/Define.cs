using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_Sample.DataClasses
{
    internal static class Define
    {
        public const string PARA_INDEX = "Index";
        public const string PARA_INFO = "Info";
        public const string PARA_MATERIAL = "Material";
        public const string PARA_HEIGHT = "Height";

        public static string[] GetAllParaNames => new string[]
        {
            PARA_INDEX,
            PARA_INFO,
            PARA_MATERIAL,
            PARA_HEIGHT
        };
    }
}