using System;

namespace HumanCommuntity.Data.HumanBeing.Doctor
{
    public class Doctor : HumanBeing
    {
        public string Major { get; set; }
        public override void Communicate()
        {

            Console.WriteLine("Doctor explains how to cure that kind of illness");
        }
        public void Job() { Console.WriteLine("Do healthchecking"); }
    }
}
