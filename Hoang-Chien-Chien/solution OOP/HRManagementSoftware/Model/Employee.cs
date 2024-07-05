using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HRManagementSoftware.Global;

namespace HRManagementSoftware
{
    [Serializable]
    public class Employee
    {
        
        private Guid idSubsidiaryCom;
        private Guid idDep;
        private Guid id;
        private string name;
        private DateTime? dob;
        private string citizenId;
        private DateTime? contractDate;
        private string workingTime;
        private ChucVu position;
        private double salary;
        private double score;

        public Employee() { 
            this.ID = Guid.NewGuid();
        }
        public Employee(Guid idSubsidiaryCom, Guid idDep, string name, DateTime? dob, string citizenId, DateTime? contractDate, string workingTime, ChucVu position, double salary, double score)
        {
            this.IdSubsidiaryCom = idSubsidiaryCom;
            this.IdDep = idDep;
            this.ID = Guid.NewGuid();
            this.Name = name;
            this.Dob = dob;
            this.CitizenId = citizenId;
            this.ContractDate = contractDate;
            this.WorkingTime = workingTime;
            this.Position = position;
            this.Salary = salary;
            this.Score = score;       
        }
        public Guid IdSubsidiaryCom { get { return idSubsidiaryCom; } set { idSubsidiaryCom = value; } }
        public Guid IdDep { get { return idDep; } set { idDep = value; } }
        public Guid ID { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public DateTime? Dob { get { return dob; } set { dob = value; } }
        public string CitizenId { get { return citizenId; } set { citizenId = value; } }
        public DateTime? ContractDate { get { return contractDate; } set { contractDate = value; } }
        public string WorkingTime { get { return workingTime; } set { workingTime = value; } }
        public double Salary { get { return salary; } set { salary = value; } }
        public double Score { get { return score; } set { score = value; } }
        public ChucVu Position { get { return position; } set { position = value; } }

    }
}
