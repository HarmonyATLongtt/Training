using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary2.Data.FrameData
{
    public class PointData
    {
        public string Id { get; set; }
        public XYZ Point { get; set; }

        public PointData() 
        {
            Point = new XYZ();
            Id = string.Empty;
        }
    }
}
