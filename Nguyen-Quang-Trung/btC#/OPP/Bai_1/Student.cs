using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class Student : Interface_IPerson
    {
        public string Class { get; set; }
        public string School { get; set; }
    }
}