using NguyenVanViet.Core;
using NguyenVanViet.Models;
using OOP.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OOP.Programs
{
    public class Bai_3 : IProgram
    {
        private List<IPerson> listPerson;

        public void Init()
        {
            TaxData taxData = new TaxData();
            listPerson = new List<IPerson>
            {
                new Student(1, "Nguyen Van A", 20, 125000f, "12A1", "Ngo Si Lien", taxData),
                new Teacher(2, "Nguyen Thi B", 25, 250000, "Le Loi", taxData),
                new Employee(3, "Nguyen Van C", 23, 2000000, "ABC Company", "Intern Web", taxData)
            };
        }

        public void Output()
        {
            if (listPerson != null)
            {
                List<Student> students = listPerson.Where(e => e is Student).Cast<Student>().ToList();
                List<Teacher> teachers = listPerson.Where(e => e is Teacher).Cast<Teacher>().ToList();
                List<Employee> employees = listPerson.Where(e => e is Employee).Cast<Employee>().ToList();

                Console.WriteLine("Student: " + students.Count());
                foreach (var item in students)
                {
                    item.GetInfo();
                }

                Console.WriteLine("Teacher: " + teachers.Count());
                foreach (var item in teachers)
                {
                    item.GetInfo();
                }

                Console.WriteLine("Employee: " + employees.Count());
                foreach (var item in employees)
                {
                    item.GetInfo();
                }
            }
        }
    }
}