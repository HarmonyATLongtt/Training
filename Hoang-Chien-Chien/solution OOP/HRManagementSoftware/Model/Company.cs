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
        public string Name
        { get { return name; } set { name = value; } }
        private string address;
        public string Address
        { get { return address; } set { address = value; } }
        private Employee president;
        public Employee President
        { get { return president; } set { president = value; } }
        private DateTime? dob;
        public DateTime? Dob
        { get { return dob; } set { dob = value; } }
        private string businessLicense;
        public string BusinessLicense
        { get { return businessLicense; } set { businessLicense = value; } }
        private string description;
        public string Description
        { get { return description; } set { description = value; } }
        private int numberOfEmp;
        public int NumberOfEmp
        { get { return numberOfEmp; } set { numberOfEmp = value; } }
        private int numberOfSub;
        public int NumberOfSub
        { get { return numberOfSub; } set { numberOfSub = value; } }
    }
}