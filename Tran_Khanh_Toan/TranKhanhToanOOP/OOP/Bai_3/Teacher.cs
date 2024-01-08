using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Teacher : Person
    {
        public string School { get; set; }
        public Teacher() { }

        public Teacher(int Id, string Name, int Age, double InCome, double TaxCoe ,string School)
            : base(Id, Name, Age ,InCome, TaxCoe)
        {
            this.School = School;
        }

        public Teacher(int Id, string Name, int Age, double InCome, TaxData taxData, string School)
            : base(Id, Name, Age, InCome, taxData)
        {
            this.School = School;
        }

        public new string GetInfo()
        {
            return $" Teacher: {Id}_{Name}_{Age}_{School}_{InCome}_{TaxCoe}";
        }


    }
}
