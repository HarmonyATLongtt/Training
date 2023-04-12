using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class Employee : Interface_IPerson
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }
    }
}