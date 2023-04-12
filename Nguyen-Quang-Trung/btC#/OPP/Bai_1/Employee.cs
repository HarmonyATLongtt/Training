using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class Employee : Interface_IPerson
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public override string ToString()
        {
            string baseIfo = base.ToString();
            string info = string.Format("{0}\t{1}\t{2}", baseIfo, Company, JobTitle);
            return info;
        }
    }
}