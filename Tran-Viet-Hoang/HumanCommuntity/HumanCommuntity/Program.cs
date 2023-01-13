using System;

namespace HumanCommuntity
{


    public class HumanBeing 
    {
        public string Name { get; set; }
        public double Age { get; set; }
        public double Height { get; set; }
        public string Sex { get; set; }
        public virtual void Communicate () { Console.WriteLine("Talk"); }
        public virtual void Eating () { Console.WriteLine("Eat"); }

        public virtual void Sleeping () { Console.WriteLine("Sleep"); }

        public virtual void Job() { Console.WriteLine("Do things");}
    }

    public class Student : HumanBeing 
    {
        public string Class { get; set; }
        public override void Communicate()
        {

            Console.WriteLine("Students are so noisy");
        }
        public void Job() { Console.WriteLine("Do homework"); }
    }

    public class Teacher : HumanBeing 
    { 
        public string Subject { get; set; }
        public override void Communicate()
        {

            Console.WriteLine("Teacher required all of student to keep quite");
        }
        public void Job() { Console.WriteLine("Do teaching"); }
    }

    public class Doctor : HumanBeing
    {
        public string Major { get; set; }
        public override void Communicate()
        {

            Console.WriteLine("Doctor explains how to cure that kind of illness");
        }
        public void Job() { Console.WriteLine("Do healthchecking"); }
    }

    internal class Program
    {
        static void Main(string[] args)
        {

            HumanBeing hocsinh = new Student { Name = "cháu Bo tiểu học" , Age = 10 , Sex = "Nam" };
            HumanBeing bacsi = new Doctor { Name = "bác sĩ Đông" , Age = 38, Sex = "Nam" };
            HumanBeing giaovien = new Teacher { Name = "cô giáo Kim", Age = 32, Sex = "Nữ"  };

            Console.WriteLine(hocsinh.Name + " " + hocsinh.Age + " tủi, giới tính: " + hocsinh.Sex);
            hocsinh.Communicate(); //override
            hocsinh.Job();         //not override
            Console.Write("\n");

            Console.WriteLine(bacsi.Name + " " + bacsi.Age + " tủi, giới tính: " + bacsi.Sex);
            bacsi.Communicate();
            bacsi.Job();
            Environment.NewLine.Clone();
            Console.Write("\n");

            Console.WriteLine(giaovien.Name + " " + giaovien.Age + " tủi, giới tính: " + giaovien.Sex);
            giaovien.Communicate();
            giaovien.Job();

            Console.ReadLine();
        }
    }
}
