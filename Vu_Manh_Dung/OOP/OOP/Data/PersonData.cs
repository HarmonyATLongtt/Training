using OOP.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP.Data
{
    public static class PersonData
    {

        private static void SelectTypePerson(ref Person p)
        {
            Console.WriteLine("=====Select=====");
            Console.WriteLine("1. Student");
            Console.WriteLine("2. Teacher");
            Console.WriteLine("3. Employee");
            Console.Write("Choose: ");
            int select = int.Parse(Console.ReadLine());
            switch (select)
            {
                case 1:
                    p = new Student();
                    break;
                case 2:
                    p = new Teacher();
                    break;
                case 3:
                    p = new Employee();
                    break;
                default:
                    Console.WriteLine("Please select again...");
                    SelectTypePerson(ref p);
                    break;
            }
            
        }
        public static void Init(List<Person> list)
        {
            list.Add(new Student("sv01", "Student 1", 20, 0, "IT1", "HaUI"));
            list.Add(new Student("sv04", "Student 4", 20, 0, "IT2", "HaUI"));
            list.Add(new Student("sv03", "Student 3", 22, 0, "IT4", "HaUI"));
            list.Add(new Student("sv02", "Student 2", 21, 0, "IT3", "HaUI"));
            list.Add(new Student("sv06", "Student 6", 19, 0, "IT4", "HaUI"));
            list.Add(new Student("sv05", "Student 5", 18, 0, "IT3", "HaUI"));

            list.Add(new Teacher("tc02", "Teacher 2", 30, 13000000, "HaUI"));
            list.Add(new Teacher("tc03", "Teacher 3", 27, 15000000, "HaUI"));
            list.Add(new Teacher("tc04", "Teacher 4", 35, 26000000, "HaUI"));
            list.Add(new Teacher("tc01", "Teacher 1", 38, 29000000, "HaUI"));

            list.Add(new Employee("epl01", "Employee 1", 24, 10000000, "HarmonyAT", "IT"));
            list.Add(new Employee("epl03", "Employee 3", 26, 17000000, "HarmonyAT", "IT"));
            list.Add(new Employee("epl02", "Employee 2", 25, 20000000, "HarmonyAT", "IT"));

            Console.Write("Number of person: ");
            int amount = int.Parse(Console.ReadLine());
            int element = 0;
            while(element < amount)
            {
                Person p = null;
                SelectTypePerson(ref p);
                p.Init();
                list.Add(p);
                element++;
            }
        }

        public static void Output(List<Person> list)
        {
            List<Student> stlist = list.Where(p => p is Student).Cast<Student>().ToList();
            List<Teacher> tclist = list.Where(p => p is Teacher).Cast<Teacher>().ToList();
            List<Employee> emplist = list.Where(p => p is Employee).Cast<Employee>().ToList();

            Console.WriteLine("\nStudent :" + stlist.Count);
            Student.Title();
            stlist.ForEach(p => p.GetInfo());
            Console.WriteLine("\nTeacher :" + tclist.Count);
            Teacher.Title();
            tclist.ForEach(p => p.GetInfo());
            Console.WriteLine("\nEmployee :" + emplist.Count);
            Employee.Title();
            emplist.ForEach(p => p.GetInfo());
        }
    }
}