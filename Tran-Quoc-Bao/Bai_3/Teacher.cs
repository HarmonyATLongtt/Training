using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Teacher : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
        public double Income { get; set; }

        public string GetInfo(TaxData taxData)
        {
            double taxCoe = taxData.GetTaxCoe(Age, Income);
            double tax = Income * taxCoe;
            return $"Teacher: id :{Id} name :{Name}  Age :{Age}  School :{School} Income : {Income:C} Tax: {tax:C}";
        }
    }
}
