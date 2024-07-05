using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSoftware
{
    public class DepartmentDTO
    {
        private Guid idSubsidiaryCom;
        private Guid id;
        private string name;
        private string managerName;
        private string deputyName;

        public Guid IdSubsidiaryCom { get { return idSubsidiaryCom; } set { idSubsidiaryCom = value; } }
        public Guid ID { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public string ManagerName { get { return managerName; } set { managerName = value; } }
        public string DeputyName { get {return deputyName; } set { deputyName = value; } }
    }
}
