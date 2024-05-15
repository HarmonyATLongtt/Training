using Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



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
              
                
            }
            else if (person is Teacher)
            {
                teacherCount++;
                
            }
            else if (person is Employee)
            {
                employeeCount++;
               
            }
            Console.WriteLine(person.GetInfo(taxData));
        }

        Console.WriteLine($"Student: {studentCount}");
        Console.WriteLine($"Teacher: {teacherCount}");
        Console.WriteLine($"Employee: {employeeCount}");
 
Console.ReadKey();
    }
}