using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    internal class Student : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string Class { get; set; }
        public string School { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public Student(int id, string name, int age, double income, double taxCoe, string @class, string school)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxCoe;
            Class = @class;
            School = school;
        }

        public string GetInfo()
        {
            return $"Id={Id}, Name={Name}, Age={Age}, Income={Income}, TaxCoe={TaxCoe}, Class={Class}, School={School}";
        }
        public double GetTax()
        {
            return 0;
        }
    }
}
