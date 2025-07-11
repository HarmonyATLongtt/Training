using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FirstCommand.Support.GenericClass.ComparerUtils
{
    public static class Comparers
    {
        public static readonly IEqualityComparer<XYZ> XYZ = new XYZComparer();
        public static readonly IEqualityComparer<Line> Line = new LineEqualityComparer();
    }
}