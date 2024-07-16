using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSoftware
{
    [Serializable]
    public class Department
    {
        
        private Guid idSubsidiaryCom;
        public Guid IdSubsidiaryCom { get { return idSubsidiaryCom; } set { idSubsidiaryCom = value; } }
        private Guid id;
        public Guid Id { get { return id; } set { id = value; } }
        private string name;
        public string Name { get { return name; } set { name = value; } }
        private Employee manager;
        public Employee Manager { get { return manager; } set { manager = value; } }
        public Employee Deputy { get { return deputy; } set { deputy = value; } }
        private Employee deputy;
        
        private int numberOfEmp;
        public int NumberOfEmp { get { return numberOfEmp; } set { numberOfEmp = value; } }

        public Department() { 
            this.Id = Guid.NewGuid();
        }

        public Department(Guid idSubsidiaryCom, string name, int numberOfEmp)
        {
            this.idSubsidiaryCom = idSubsidiaryCom;
            this.Id = Guid.NewGuid();
            this.Name = name;            
            this.NumberOfEmp = numberOfEmp;
            
        }
       
       
       
       
        
    }
}
