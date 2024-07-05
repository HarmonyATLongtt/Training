using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSoftware
{
    public class CompanyDTO
    {
        private Guid id;
        private string name;
        private string presidentName;

        public Guid Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string PresidentName { get { return presidentName; } set { presidentName = value; } }
    }
}
