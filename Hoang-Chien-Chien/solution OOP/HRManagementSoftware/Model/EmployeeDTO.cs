using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HRManagementSoftware.Global;

namespace HRManagementSoftware
{
    public class EmployeeDTO
    {
        private Guid id;
        private string name;        
        private string position;      
        private double score;

        public Guid Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }      
        public double Score { get { return score; } set { score = value; } }
        public string Position { get { return position; } set { position = value; } }
    }
}
