using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Student : Person
    {
        public string Class { get; set; }
        public string School { get; set; }

        public Student()
        {
            
        }

        public Student(int Id, string Name, int Age, string Class, string School)
            : base(Id, Name, Age)
        {
            this.Class = Class;
            this.School = School;
        }
        public new string GetInfo()
        {
            return ("Student: " + this.Id + "_" + this.Name + "_" + this.Age + "_" + this.School + "_"  );
        }

    }
}
