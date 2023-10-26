using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    internal interface IPerson
    {
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        double Income { get; set; }
        double TaxCoe { get; set; }
        string Class { get; set; }
        string School { get; set; }
        string Company { get; set; }
        string JobTitle { get; set; }
        double GetTax();
        string GetInfo();
    }
}