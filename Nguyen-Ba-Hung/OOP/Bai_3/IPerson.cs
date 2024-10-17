using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal interface IPerson
    {
        string Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        int Income { get; set; }
        double TaxCoe { get; set; }
        string GetInfo();
    }
}
