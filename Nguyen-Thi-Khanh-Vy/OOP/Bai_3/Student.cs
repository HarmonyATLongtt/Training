using Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP.Bai_3
{
    public class Student : IPerson
    {
       

        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Class {  get; set; }
        public string School {  get; set; }

        public Student() { }

        public Student(string id, string name, int age, string school, string @class)
        {
            ID = id;
            Name = name;
            Age = age;
            School = school;
            Class = @class;
        }
        private static string GetInput(string prompt)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Gia tri nhap khong duoc de trong.Vui long nhap lai!");
                }
            } while (string.IsNullOrWhiteSpace(input));
            return input;
        }

        public void Nhap()
        {
            ID = GetInput("Nhap ID:");
            Name = GetInput("Nhap Ho ten: ");

            while (true)
            {
                string ageInput = GetInput("Nhap tuoi: ");
                if (int.TryParse(ageInput, out int age) && age > 0)
                {
                    Age = age;
                    break;
                }
                Console.WriteLine("Tuoi khong hop le vui lpng nhap lai");
            }
            School = GetInput("Nhap truong: ");
            Class = GetInput("Nhap lop: ");
        }
        public string GetInfo()
        {
            return $"ID: {ID}, Name: {Name}, Age: {Age}, School: {School}, Class: {Class}";
        }
    }
}
