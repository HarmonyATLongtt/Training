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
        public Guid Id
        { get { return id; } set { id = value; } }
        private string name;
        public string Name
        { get { return name; } set { name = value; } }
        private string position;
        public string Position
        { get { return position; } set { position = value; } }
        private double score;
        public double Score
        { get { return score; } set { score = value; } }
    }
}