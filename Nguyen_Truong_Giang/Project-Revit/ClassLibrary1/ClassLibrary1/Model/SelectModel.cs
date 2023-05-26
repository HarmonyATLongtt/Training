using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ClassLibrary1.Model
{
    class SelectModel
    {
        public Document Doc;

        public SelectModel(Document doc)
        {
            Doc = doc;
        }
    }
}
