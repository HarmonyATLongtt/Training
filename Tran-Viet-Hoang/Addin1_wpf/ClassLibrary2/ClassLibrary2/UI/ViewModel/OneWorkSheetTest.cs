using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using ClassLibrary2.UI.Views;
using ClassLibrary2.UI.ViewModel;
using System.Windows.Input;


namespace ClassLibrary2
{
    internal class OneWorkSheetTest
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Class { get; set; }
        
        public OneWorkSheetTest() { }
    }
}
