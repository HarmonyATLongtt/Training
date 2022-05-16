using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;

namespace Bai4.Model
{
    public class MainModel
    {
        public bool CubeSelect { get; set; }
        public bool SphereSelect { get; set; }
        public double CubeLength { get; set; }
        public double SphereRadius { get; set; }
        public MainModel(Document doc) { }
    }
}
