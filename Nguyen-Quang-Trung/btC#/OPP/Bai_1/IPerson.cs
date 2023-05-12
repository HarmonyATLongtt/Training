using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    interface IPerson
    {
        int ID { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        double Income { get; set; }
        double TaxCoe { get; set; } // hệ số thuế

        bool Equals(Person p);

        double GetTax(double Income, double TaxCoe);

        string GetInfor(string key, string value);

        string ToString();
    }
}