using System;

namespace HumanCommuntity.Data.HumanBeing.Student
{
    public class Student : HumanBeing
    {
        public string Class { get; set; }
        public override void Communicate()
        {

            Console.WriteLine("Students are so noisy");
        }

        
        public void Job() { Console.WriteLine("Do homework"); }
    }
}
