using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Employee : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public double Income { get; set; }

        public string GetInfo(TaxData taxData)
        {
            double taxCoe = taxData.GetTaxCoe(Age, Income);
            double tax = Income * taxCoe;
            return $"Employee: id :{Id} name :{Name} age : {Age} Company :{Company}  Job Title :{JobTitle} Income : {Income:C} Tax: {tax:C}";
        }
    }
}
