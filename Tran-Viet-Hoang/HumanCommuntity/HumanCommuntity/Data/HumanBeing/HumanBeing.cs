using System;

namespace HumanCommuntity.Data.HumanBeing
{
    public class HumanBeing
    {
        public string Name { get; set; }
        public double Age { get; set; }
        public double Height { get; set; }
        public string Sex { get; set; }
        public virtual void Communicate() { Console.WriteLine("Talk"); }
        public virtual void Eating() { Console.WriteLine("Eat"); }

        public virtual void Sleeping() { Console.WriteLine("Sleep"); }

        public virtual void Job() { Console.WriteLine("Do things"); }
    }
}
