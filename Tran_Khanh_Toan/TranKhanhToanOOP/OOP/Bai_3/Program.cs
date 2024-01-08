using Bai_3;
using System;
using System.Collections.Generic;

namespace Bai_3 // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Person> people = Init();
            Output(people);
        }

        static List<Person> Init()
        {
            TaxData taxData = new TaxData();
            List<Person> people = new List<Person>();
            people.Add(new Student(01, "Tran Khanh Toan", 22, "CNTT" ,"HaNoi school"));
            people.Add(new Student(02, "Tran Khanh Toan0", 22, "CNTT" ,"HaNoi school"));
            people.Add(new Student(03, "Tran Khanh Toan00", 22, "CNTT" ,"HaNoi school"));

            people.Add(new Teacher(04, "Tran Khanh Toan1", 23, 2000000, taxData, "HaNoi School1"));
            people.Add(new Teacher(05, "Tran Khanh Toan11", 24, 250000000, taxData, "HaNoi School1"));
            people.Add(new Teacher(06, "Tran Khanh Toan11", 26, 1000000, taxData, "HaNoi School1"));
            people.Add(new Teacher(07, "Tran Khanh Toan11", 26, 1000000, taxData, "HaNoi School1"));

            people.Add(new Employee(08, "Tran Khanh Toan2", 26, 100000000, taxData, "Company A", "No desc"));
            people.Add(new Employee(09, "Tran Khanh Toan22", 22, 30000000, taxData, "Company B", "No desc"));
            people.Add(new Employee(010, "Tran Khanh Toan222", 24, 1000000, taxData, "Company B", "No desc"));
            people.Add(new Employee(011, "Tran Khanh Toan2222", 21, 40000000, taxData, "Company C", "Company"));
            people.Add(new Employee(012, "Tran Khanh Toan22222", 26, 50000000, taxData, "Company C", "No desc"));


            return people;
        }

        static void Output(List<Person> people)
        {
            int s1 = 0, s2 = 0, s3 = 0;
            foreach(Person person in people)
            {
                if(person != null)
                {
                    if (person is Student)
                    {
                        s1++;
                    }
                    if(person is Teacher)
                    {
                        s2++;
                    }
                    if(person is Employee)
                    {
                        s3++;
                    }
                }
                else
                {
                    Console.WriteLine("Empty");
                }
            }




            List<Student> StudentList = people.Where(p => p is Student).Cast<Student>().ToList();
            List<Teacher> TeacherList = people.Where(p => p is Teacher).Cast<Teacher>().ToList();
            List<Employee> EmployeeList = people.Where(p => p is Employee).Cast<Employee>().ToList();

            Console.WriteLine("Student: " + s1);
            StudentList.ForEach(p => Console.WriteLine(p.GetInfo()));
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Teacher: " + s2);
            TeacherList.ForEach(p => Console.WriteLine(p.GetInfo()));
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Employee: " + s3);
            EmployeeList.ForEach(p => Console.WriteLine(p.GetInfo()));
        }
    }
}