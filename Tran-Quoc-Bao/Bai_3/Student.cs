using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Student : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Class { get; set; }
        public string School { get; set; }

        public string GetInfo(TaxData taxData)
        {
            return $"Student: ID :{Id} Name: {Name} Age: {Age} School : {School}  Class :{Class}";
        }
    }
}
