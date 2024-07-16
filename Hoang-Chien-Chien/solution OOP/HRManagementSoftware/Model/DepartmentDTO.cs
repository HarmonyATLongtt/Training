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
        public Guid IdSubsidiaryCom
        { get { return idSubsidiaryCom; } set { idSubsidiaryCom = value; } }
        private Guid id;
        public Guid ID
        { get { return id; } set { id = value; } }
        private string name;
        public string Name
        { get { return name; } set { name = value; } }
        private string managerName;
        public string ManagerName
        { get { return managerName; } set { managerName = value; } }
        private string deputyName;
        public string DeputyName
        { get { return deputyName; } set { deputyName = value; } }
    }
}