using System;
using System.Collections.Generic;
using System.Text;

namespace WPFAPP.Model
{
    public class Student : Person
    {
        public double Income { get; set; }

        public double TaxCode { get; set; }
        public string School { get; set; }
        public string Class { get; set; }
    }
}
