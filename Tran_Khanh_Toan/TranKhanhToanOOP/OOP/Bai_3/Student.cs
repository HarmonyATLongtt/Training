using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Student : Person, IPerson
    {
        public string Class { get; set; }
        public string School { get; set; }

        public Student()
        {
            
        }

        public Student(int Id, string Name, int Age, double InCome, double TaxCoe, string Class, string School)
            : base(Id, Name, Age, InCome, TaxCoe)
        {
            this.Class = Class;
            this.School = School;
        }
        public void GetInfo()
        {
            Console.WriteLine("_" + this.Id + "_" + this.Name + "_" + this.Age + "_" + this.School + "_"  );
        }

        public double GetTax()
        {

        }
    }
}
