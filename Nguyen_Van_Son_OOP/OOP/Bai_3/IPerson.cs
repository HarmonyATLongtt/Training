using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal interface IPerson
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }
        public void GetInfo();
    }
}
