using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_1.Models
{
    public class Data
    {
        public DataTable data { get; set; }
        public Data()
        {
            data = new DataTable();
        }
    }
}
