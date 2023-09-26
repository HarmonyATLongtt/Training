using NguyenVanViet.Core;
using NguyenVanViet.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace NguyenVanViet.Controls
{
    // Because program class use to all excercise so we should create a class to use together
    public class Program
    {
        private List<Person>? li;

        private List<IPerson>? listPerson;

        // Because require of each exercise is different so we must have something to distinguish them
        // In here, I create an enum to store mark to distinguish exercises
        public void Init(bool isIPerson, ExerciseEnum exerciseEnum)
        {
            if (!isIPerson)
            {
                if (exerciseEnum == ExerciseEnum.Bai_1)
                {
                    li = new List<Person>
                    {
                        new Person(1, "Nguyen Van A", 19, 1250000, 0.3f),
                        new Person(2, "Nguyen Van B", 20, 1000000, 0.1f),
                        new Person(3, "Nguyen Van C", 18, 2100000, 0.4f),
                        new Person(4, "Nguyen Van D", 21, 1500000, 0.2f),
                        new Person(5, "Nguyen Van E", 19, 3000000, 0.5f)
                    };
                }
                else if (exerciseEnum == ExerciseEnum.Bai_2)
                {
                    TaxData taxData = new TaxData();
                    li = new List<Person>
                    {
                        new Person(1, "Nguyen Van A", 19, 1250000, taxData),
                        new Person(2, "Nguyen Van B", 20, 10000000, taxData),
                        new Person(3, "Nguyen Van C", 18, 21000000, taxData),
                        new Person(4, "Nguyen Van D", 21, 15000000, taxData),
                        new Person(5, "Nguyen Van E", 19, 30000000, taxData)
                    };
                }
            }
            else
            {
                TaxData taxData = new TaxData();
                listPerson = new List<IPerson>
                {
                    new Student(1, "Nguyen Van A", 20, 125000f, "12A1", "Ngo Si Lien", taxData),
                    new Teacher(2, "Nguyen Thi B", 25, 250000, "Le Loi", taxData),
                    new Employee(3, "Nguyen Van C", 23, 2000000, "ABC Company", "Intern Web", taxData)
                };
            }
        }

        // The same Init method, we must distinguish exercise1, 2 and 3
        public void Output(bool isIPerson)
        {
            if (!isIPerson)
            {
                if (li != null)
                    foreach (var item in li)
                    {
                        Console.WriteLine("ID: {0}, Name: {1}, Tax: {2}", item.Id, item.Name, item.TaxCoe);
                    }
                else
                    Console.WriteLine("Don't contain data!");
            }
            else
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
}