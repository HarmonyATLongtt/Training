using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class Person
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; } // hệ số thuế

        public bool Equals(Person p)
        {
            if (p == null)
            {
                return false;
            }

            return this == p; // ??? trùng hoàn toàn thì mới trả về true, nếu so sánh theo thuộc tính thì chỉ nên so sánh id
        }

        public double GetTax(double Income, double TaxCoe)
        {
            return Income * TaxCoe;
        }
    }
}