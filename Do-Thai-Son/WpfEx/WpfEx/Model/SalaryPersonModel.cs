using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfEx.Model
{
    public class SalaryPersonModel
    {
        public string Name { get; set; }

        public DataTable Data;

        public SalaryPersonModel(DataTable table)
        {
            if (table != null)
            {
                Name = table.TableName;
                Data = table;
            }
            else
            {
                Name = string.Empty;
                Data = new DataTable();
            }
        }
    }
}