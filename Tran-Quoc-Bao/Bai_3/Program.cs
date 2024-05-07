using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TaxData
{ 

    public double GetTaxCoe(int age, double income)
    {
        if (age < 18)
        {
            return 0;
        }
        else
        {
            if (income <= 9000000)
                return 0.05;
            else if (income <= 15000000)
                return 0.1;
            else if (income <= 20000000)
                return 0.15;
            else if (income <= 30000000)
                return 0.2;
            else
                return 0;
        }

    } 
}
public interface IPerson
{
    string GetInfo();
}

public class Student : IPerson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Class { get; set; }
    public string School { get; set; }

    public string GetInfo()
    {
        return $"Student: {Id} {Name} {Age} {School} {Class}";
    }
}
public class Teacher : IPerson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string School { get; set; }
    public double Income { get; set; }

    public string GetInfo()
    {
        return $"Teacher: {Id} {Name} {Age} {School} {Income}";
    }
}
public class Employee : IPerson
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Company { get; set; }
    public string JobTitle { get; set; }
    public double Income { get; set; }

    public string GetInfo()
    {
        return $"Employee: {Id} {Name} {Age} {Company} {JobTitle} {Income:C}";
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        List<IPerson> people = new List<IPerson>();

        TaxData taxData = new TaxData();

      
        Student student1 = new Student { Id = 11, Name = "Duc", Age = 15, School = " THPT CVA", Class = " 12A2" };
        people.Add(student1);

        
        Teacher teacher1 = new Teacher { Id = 22, Name = "Nhung", Age = 23, School = "THPT CVA", Income = 20000000 };
        people.Add(teacher1);

        
        Employee employee1 = new Employee { Id = 33, Name = "Binh", Age = 27, Company = "HAT", JobTitle = "Engineer", Income = 25000000 };
        people.Add(employee1);

        Output(people, taxData);
    }

    public static void Output(List<IPerson> people, TaxData taxData)
    {
        int studentCount = 0, teacherCount = 0, employeeCount = 0;

        foreach (var person in people)
        {
            if (person is Student)
            {
                studentCount++;
                Student student = (Student)person;
                Console.WriteLine($"Student: ");
                Console.WriteLine($"ID: {student.Id}, Name: {student.Name}, Age: {student.Age}, School: {student.School} , Class : {student.Class}");
            }
            else if (person is Teacher)
            {
                teacherCount++;
                Teacher teacher = (Teacher)person;
                double taxCoe = taxData.GetTaxCoe(teacher.Age, teacher.Income);
                double tax = teacher.Income * taxCoe;
                Console.WriteLine($"Teacher: ");
                Console.WriteLine($"ID: {teacher.Id}, Name: {teacher.Name}, Age: {teacher.Age}, School: {teacher.School}, Income: {teacher.Income}, Tax: {tax}");
            }
            else if (person is Employee)
            {
                employeeCount++;
                Employee employee = (Employee)person;
                double taxCoe = taxData.GetTaxCoe(employee.Age, employee.Income);
                double tax = employee.Income * taxCoe;
                Console.WriteLine($"Employee: ");
              
                Console.WriteLine($"ID: {employee.Id}, Name: {employee.Name}, Age: {employee.Age}, Company: {employee.Company}, JobTitle: {employee.JobTitle}, Income: {employee.Income}, Tax: {tax}");
            }
        }

        Console.WriteLine($"Student: {studentCount}");
        Console.WriteLine($"Teacher: {teacherCount}");
        Console.WriteLine($"Employee: {employeeCount}");
 
Console.ReadKey();
    }
}