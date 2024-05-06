using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateFamily.Model
{
  public  class ItemCheckModel
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }

        public XYZ Point { get; set; }
        public ItemCheckModel()
        {

        }
    }
}
