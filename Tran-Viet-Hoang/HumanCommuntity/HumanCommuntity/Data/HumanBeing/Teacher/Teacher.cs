using System;

namespace HumanCommuntity.Data.HumanBeing.Teacher
{
    public class Teacher : HumanBeing
    {
        public string Subject { get; set; }
        public override void Communicate()
        {

            Console.WriteLine("Teacher required all of student to keep quite");
        }
        public void Job() { Console.WriteLine("Do teaching"); }
    }
}
