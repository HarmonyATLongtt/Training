using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSoftware
{
    [Serializable]
    public class Company
    {
        private string name;
        private string address;
        private Employee president;
        private DateTime? dob;
        private string businessLicense;
        private string description;
        private int numberOfEmp;
        private int numberOfSub;

        public string Name { get{ return name; } set { name = value; } }
        public string Address { get{ return address; } set { address = value; } }
        public Employee President { get{ return president; } set { president = value; } }
        public DateTime? Dob { get{ return dob; } set { dob = value; } }
        public string BusinessLicense { get{ return businessLicense; } set { businessLicense = value; } }
        public string Description { get{ return description; } set { description = value; } }
        public int NumberOfEmp { get{ return numberOfEmp; } set { numberOfEmp = value; } }
        public int NumberOfSub { get{ return numberOfSub; } set {numberOfSub = value; } }
    }
}
