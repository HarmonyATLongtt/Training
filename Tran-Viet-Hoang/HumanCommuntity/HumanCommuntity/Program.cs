using HumanCommuntity.Data.HumanBeing;
using HumanCommuntity.Data.HumanBeing.Doctor;
using HumanCommuntity.Data.HumanBeing.Student;
using HumanCommuntity.Data.HumanBeing.Teacher;
using System;

namespace HumanCommuntity
{
    public class Program
    {
        private static void Main(string[] args)
        {
            HumanBeing hocsinh = new Student { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };
            Student hocsinh1 = new Student { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };
            HumanBeing bacsi = new Doctor { Name = "bác sĩ Đông", Age = 38, Sex = "Nam" };
            HumanBeing giaovien = new Teacher { Name = "cô giáo Kim", Age = 32, Sex = "Nư" };

            Console.WriteLine(hocsinh.Name + " " + hocsinh.Age + " tuôi, giơi tính: " + hocsinh.Sex);
            hocsinh.Communicate(); //not override: Talk
            hocsinh1.Communicate(); // hiding....: Students are so noisy

            hocsinh.Job();         //not override
            Console.Write("\n");

            Console.WriteLine(bacsi.Name + " " + bacsi.Age + " tuôi, giơi tính: " + bacsi.Sex);
            bacsi.Communicate(); //override
            bacsi.Job();
            Console.Write("\n");

            Console.WriteLine(giaovien.Name + " " + giaovien.Age + " tuôi, giơi tính: " + giaovien.Sex);
            giaovien.Communicate(); //override
            giaovien.Job();

            Console.ReadLine();
        }
    }
}