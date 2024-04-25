using System;
using System.Collections.Generic;
using System.Text;

namespace WPFAPP.Model
{
    public class Teacher : Person
    {
        public double TaxCode { get; set; }
        public double Income { get; set; }
        public string School { get; set; }
    }
}
