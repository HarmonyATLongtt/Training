using Bai_2;
using Bai_3;
using System;

class Program
{
    static void Main(string[] args)
    {
        Program program = new Program();
        List<IPerson> lp = program.Init();
        program.Output(lp);

        return;
    }

    public List<IPerson> Init()
    {
        List<IPerson> people = new List<IPerson>();

        people.Add(new Student ( "A1", "Bang Qa", 15, 10000000, "DSA", "Cap 3"));
        people.Add(new Student ("A2", "La Luot", 19, 10000000, "SE", "Dai Hoc"));

        people.Add(new Teacher("T1", "Nguyen", 27, 15000000, "Dai hoc"));
        people.Add(new Teacher("T2", "Tran", 40, 26000000, "Dai hoc"));

        people.Add(new Employee("E1", "TDH", 21, 1200000, "LG", "Dev"));
        people.Add(new Employee("E2", "OQ", 24, 8000000, "Samsung", "HR"));

        return people;
    }

    public void Output(List<IPerson> p)
    {
        int studentCount = 0;
        int teacherCount = 0;
        int employeeCount = 0;

        foreach(IPerson person in p)
        {
            if(person is Student)
            {
                studentCount++;
            }
            else if(person is Teacher)
            {
                teacherCount++;
            }
            else if (person is Employee)
            {
                employeeCount++;
            }
        }

        List<IPerson> temp = p;

        Console.WriteLine("Student: " + studentCount);
        Console.WriteLine("\tID\tName\tAge\tSchool\tClass");
        foreach (var person in temp)
        {
            if (person is Student)
            {
                Console.WriteLine(person.GetInfo());
            }
        }

        Console.WriteLine("Teacher: " + teacherCount);
        Console.WriteLine("\tID\tName\tAge\tSchool\tIncome\t\tTax");
        foreach (var person in temp)
        {
            if (person is Teacher)
            {
                Console.WriteLine(person.GetInfo());
            }
        }

        Console.WriteLine("Employee: " + employeeCount);
        Console.WriteLine("\tID\tName\tAge\tCompany\tTitle\tIncome\tTax");
        foreach (var person in temp)
        {
            if (person is Employee)
            {
                Console.WriteLine(person.GetInfo());
            }
        }
    }
}