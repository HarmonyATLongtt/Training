using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1.Bai_1.Model
{
    public class MainModel
    {
        public int CategoryNumber { get; set; }
        public int FamilyNumber { get; set; }
        public int TypeNumber { get; set; }

        public MainModel(int category, int family, int type)
        {
            CategoryNumber = category;
            FamilyNumber = family;
            TypeNumber = type;
        }
    }
}
