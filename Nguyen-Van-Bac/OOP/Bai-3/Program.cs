using System;
using System.Collections.Generic;

namespace Bai_3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<IPerson> people = Init();
            Output(people);
        }
        static List<IPerson> Init()
        {
            TaxData taxData = new TaxData();
            var ListPerson = new List<IPerson>()
            {           
            new Person(1, "Nam", 20, 9000000, taxData),
            new Student { Id = 2, Name = "Phú", Age = 21, School = "ABC School", Class = "12A" },
            new Teacher { Id = 3, Name = "Thảo", Age = 35, School = "XYZ School", Income = 7000000, TaxCoe = taxData.GetTaxCoe(35, 7000000) },
            new Employee { Id = 4, Name = "Hoàng", Age = 25, Company = "XXX Corporation", JobTitle = "Software Engineer", Income = 10000000, TaxCoe = taxData.GetTaxCoe(25, 10000000) }
            };
            return ListPerson;
        }
        static void Output(List<IPerson> people)
        {
            int studentCount = 0, teacherCount = 0, employeeCount = 0;

            foreach (var person in people)
            {
                if (person is Student)
                {
                    studentCount++;
                }
                else if (person is Teacher)
                {
                    teacherCount++;
                }
                else if (person is Employee)
                {
                    employeeCount++;
                }
                Console.WriteLine(person.GetInfo());
            }

            Console.WriteLine($"Student: {studentCount}");
            Console.WriteLine($"Teacher: {teacherCount}");
            Console.WriteLine($"Employee: {employeeCount}");
        }
    }
}
