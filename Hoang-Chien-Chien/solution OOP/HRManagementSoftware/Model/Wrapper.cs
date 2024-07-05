using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSoftware
{
    [Serializable]
    public class Wrapper
    {
        public Company Company { get; set; }
        public List<SubsidiaryCom> SubsidiaryComs { get; set; }
        public List<Department> Departments { get; set; }
        public List<Employee> Employees { get; set; }       

        public Wrapper() { }

        public Wrapper(Company company, List<SubsidiaryCom> subsidiaryComs, List<Department> departments, List<Employee> employees)
        {
            Company = company;
            SubsidiaryComs = subsidiaryComs;
            Departments = departments;
            Employees = employees;            
        }
    }
}
