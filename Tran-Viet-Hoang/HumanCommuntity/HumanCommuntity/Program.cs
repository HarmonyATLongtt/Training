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
            int[] index = { 1, 2, 3, 2, 4, 6, 2, 4 };
            string number = "Ta có dãy số: ";
            int count = 0;
            for(int i = 0; i < index.Length; i++) 
            {
                number += index[i] + ",";
                if(index[i] == 2) 
                {
                    count++;
                }
            }
            Console.WriteLine(number);
            Console.WriteLine("Số chữ số 2 trong dãy là:" + count);
            Console.ReadLine();

            //HumanBeing human = new HumanBeing { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };
            //HumanBeing hocsinh = new Student { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };
            //Student hocsinh1 = new Student { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };

            //Student hocsinh_undergraduate = new Undergraduate { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };
            //Undergraduate hocsinh_undergraduate1 = new Undergraduate { Name = "cháu Bo tiêu hoc", Age = 10, Sex = "Nam" };



            //HumanBeing bacsi = new Doctor { Name = "bác sĩ Đông", Age = 38, Sex = "Nam" };
            //HumanBeing giaovien = new Teacher { Name = "cô giáo Kim", Age = 32, Sex = "Nư" };

            //Console.WriteLine(human.Name + " " + human.Age + " tuôi, giơi tính: " + human.Sex);
            //human.Communicate(); //not override: Talk
            //Console.Write("\n");
            //hocsinh.Communicate(); // hiding....: Students are so noisy
            //hocsinh1.Communicate(); // hiding....: Students are so noisy
            //Console.Write("\n");
            //hocsinh_undergraduate.Communicate(); // hiding....: Students are so noisy
            //hocsinh_undergraduate1.Communicate(); // hiding....: Students are not noisy

            //human.Job();         //not override
            //Console.Write("\n");

            //Console.WriteLine(bacsi.Name + " " + bacsi.Age + " tuôi, giơi tính: " + bacsi.Sex);
            //bacsi.Communicate(); //override
            //bacsi.Job();
            //Console.Write("\n");

            //Console.WriteLine(giaovien.Name + " " + giaovien.Age + " tuôi, giơi tính: " + giaovien.Sex);
            //giaovien.Communicate(); //override
            //giaovien.Job();

            //Console.ReadLine();
        }
    }
}