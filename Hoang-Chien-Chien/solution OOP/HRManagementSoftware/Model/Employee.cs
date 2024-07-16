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
        public Guid IdSubsidiaryCom
        { get { return idSubsidiaryCom; } set { idSubsidiaryCom = value; } }
        private Guid idDep;
        public Guid IdDep
        { get { return idDep; } set { idDep = value; } }
        private Guid id;
        public Guid ID
        { get { return id; } set { id = value; } }
        private string name;
        public string Name
        { get { return name; } set { name = value; } }
        private DateTime? dob;
        public DateTime? Dob
        { get { return dob; } set { dob = value; } }
        private string citizenId;
        public string CitizenId
        { get { return citizenId; } set { citizenId = value; } }
        private DateTime? contractDate;
        public DateTime? ContractDate
        { get { return contractDate; } set { contractDate = value; } }
        private string workingTime;
        public string WorkingTime
        { get { return workingTime; } set { workingTime = value; } }
        private ChucVu position;
        public ChucVu Position
        { get { return position; } set { position = value; } }
        private double salary;
        public double Salary
        { get { return salary; } set { salary = value; } }
        private double score;
        public double Score
        { get { return score; } set { score = value; } }

        public Employee()
        {
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
    }
}