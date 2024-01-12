using Bai_2;
using Bai_3;

List<IPerson>? li;


void Init()
{
    li = new List<IPerson>
            {
                new Student(1,"Nguyen Van A",18,0,new TaxData(),"CNTT","HaUI"),
                new Teacher(2,"Nguyen Van B",28,15000000,new TaxData(),"HaUI"),
                new Employee(3,"Nguyen Van C",24,20000000,new TaxData(),"Harmony","Dev"),
                new Student(3,"Nguyen Van D",19,0,new TaxData(),"CNTT","HaUI"),
                new Employee(3,"Nguyen Van E",23,18000000,new TaxData(),"Harmony","Tester"),
                new Student(3,"Nguyen Van F",20,0,new TaxData(),"CNTT","HaUI"),
            };
}

void Output()
{
    if (li != null)
    {
        List<Student> students = li.Where(p => p is Student).Cast<Student>().ToList();
        List<Teacher> teachers = li.Where(p => p is Teacher).Cast<Teacher>().ToList();
        List<Employee> employees = li.Where(p => p is Employee).Cast<Employee>().ToList();
        Console.WriteLine("Student: " + students.Count);
        Student.Title();
        foreach (Student s in students)
        {
            s.GetInfo();
        }
        Console.WriteLine("Teacher: " + teachers.Count);
        Teacher.Title();
        foreach (Teacher t in teachers)
        {
            t.GetInfo();
        }
        Console.WriteLine("Employees: " + employees.Count);
        Employee.Title();
        foreach (Employee e in employees)
        {
            e.GetInfo();
        }
    }
    else
    {
        Console.WriteLine("No data show!");
    }
}


Init();
Output();