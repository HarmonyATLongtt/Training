using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSoftware
{
    [Serializable]
    public class SubsidiaryCom:Company
    {
        
        private Guid id;

        private List<int> idGoodEmp;
        public SubsidiaryCom()
        {
            this.Id = Guid.NewGuid();
        }
        public Guid Id { get { return id; } set { id = value; } }  
    }
}
