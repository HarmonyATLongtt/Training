using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class TaxData // Declare class TaxData has a method name GetTaxCoe to calculate Tax
    {
        public float GetTaxCoe(int age, float income) // Delcare and define method GetTaxCoe with two parameters are 'age' and 'income'
        {
            // Use ternary operator to calculate 'tax coe'. Can use if-else statement
            return age < 18 ? 0 : income <= 9000000 ? 0.05f : income <= 15000000 ? 0.1f : income <= 20000000 ? 0.15f : 0.2f;
        }
    }

    public interface IPerson // Declare interface IPerson has a method name GetInfor
    {
        public void GetInfo(); // Declare method name GetInfo to get nescess info of Person
    }

    public class Student : IPerson // Declare class Student implement interface IPerson
    {
        // Declare nescess info
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }
        public string Class { get; set; }
        public string  School { get; set; }

        public Student(int id, string name, int age, float income, string clas, string school, TaxData taxData) // Declare constructor with other params
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Class = clas;
            School = school;
            TaxCoe = taxData.GetTaxCoe(age, income); // Use instance of TaxData name taxData call GetTaxCoe method to calculate tax and assign to TaxCoe
        }

        public void GetInfo() // Override method GetInfo from interface IPerson
        {
            // Write nescess infor to window
            Console.WriteLine($"ID: {Id}, Name: {Name}, Age: {Age}, School: {School} , Class: {Class}");
        }
    }

    public class Teacher : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }
        public string School { get; set; }

        public Teacher() { }

        public Teacher(int id, string name, int age, float income, string school, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            School = school;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public void GetInfo()
        {
            Console.WriteLine($"ID: {Id}, Name: {Name}, Age: {Age}, School: {School}, Income: {Income}, Tax: {TaxCoe}");
        }
    }

    public class Employee : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public Employee() { }

        public Employee(int id, string name, int age, float income, string company, string jobTitle, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Company = company;
            JobTitle = jobTitle;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public void GetInfo()
        {
            Console.WriteLine($"ID: {Id}, Name: {Name}, Age: {Age}, Company: {Company}, Job title: {JobTitle}, Income: {Income}, Tax: {TaxCoe}");
        }
    }

    public class Program
    {
        List<IPerson> li; // Declare list type IPerson to stores data

        public void Init() // Init method to add data
        {
            TaxData taxData = new TaxData(); // Create taxData is instance of TaxData
            li = new List<IPerson> // Add data for list
            {
                new Student(1, "Nguyen Van A", 20, 125000f, "12A1", "Ngo Si Lien", taxData),
                new Teacher(2, "Nguyen Thi B", 25, 250000, "Le Loi", taxData),
                new Employee(3, "Nguyen Van C", 23, 2000000, "ABC Company", "Intern Web", taxData)
            };
        }

        public void Output() // Output method
        {
            int studentCount = 0;
            int teacherCount = 0;
            int employeeCount = 0;

            foreach(IPerson item in li ) // Use foreach loop to count total student, teacher and employee
            {
                if (item is Student)
                    studentCount++;
                else if (item is Teacher)
                    teacherCount++;
                else
                    employeeCount++;
            }

                Console.WriteLine("Student: " + studentCount); // Write notifycation about total student
            foreach(IPerson item in li ) // Use foreach loop to write all student on screen
            {
                if(item is Student) // Check if item (data) is Student
                    item.GetInfo(); // Show info
            }

                Console.WriteLine("Teacher: " + teacherCount);
            foreach (IPerson item in li)
            {
                if (item is Teacher)
                    item.GetInfo();
            }

                Console.WriteLine("Employee: " + employeeCount);
            //foreach (IPerson item in li)
            //{
            //    if (item is Employee)
            //        item.GetInfo();
            //}
            List<Employee> empl = (from x in li
                        where x is Employee
                        select x as Employee).ToList();
            List<Employee> employ =li.Where(x=> x is Employee).Cast<Employee>().ToList();
        }
    }

    public class Bai_3
    {
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Init();
            p.Output();
        }
    }
    
}
