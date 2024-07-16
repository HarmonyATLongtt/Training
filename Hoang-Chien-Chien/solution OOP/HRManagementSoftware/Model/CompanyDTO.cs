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
        public Guid Id
        { get { return id; } set { id = value; } }
        private string name;
        public string Name
        { get { return name; } set { name = value; } }
        private string presidentName;
        public string PresidentName
        { get { return presidentName; } set { presidentName = value; } }
    }
}