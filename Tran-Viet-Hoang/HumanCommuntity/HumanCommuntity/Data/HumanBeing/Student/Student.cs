using System;

namespace HumanCommuntity.Data.HumanBeing.Student
{
    public class Student : HumanBeing
    {
        public string Class { get; set; }
        public new void Communicate()
        {

            Console.WriteLine("Students are so noisy");
        }
        public void Job() { Console.WriteLine("Do homework"); }
    }
}
