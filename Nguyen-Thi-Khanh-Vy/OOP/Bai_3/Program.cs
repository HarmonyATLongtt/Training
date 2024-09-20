using Bai_3;


namespace OOP.Bai_3
{
    class Program
    {
        static List<IPerson> Init(TaxData taxData)
        {
            var p = new List<IPerson>();

            Console.WriteLine("Enter number of students, teachers, and employees:");

            Console.Write("So hoc sinh: ");
            int studentCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < studentCount; i++)
            {
                Console.WriteLine($"Thong tin hoc sinh thu {i + 1}:");
                Student student = new Student();
                student.Nhap();
                p.Add(student);
            }

            Console.Write("So giao vien: ");
            int teacherCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < teacherCount; i++)
            {
                Console.WriteLine($"Thong tin giao vien thu {i + 1}:");
                Teacher teacher = new Teacher("", "", 0, "", 0, taxData); 
                teacher.Nhap();
                p.Add(teacher);
            }

            Console.Write("So nhan vien: ");
            int employeeCount = int.Parse(Console.ReadLine());

            for (int i = 0; i < employeeCount; i++)
            {
                Console.WriteLine($"Thong tin nhan vien thu {i + 1}:");
                Employee employee = new Employee("", "", 0, "", "", 0, taxData); 
                employee.Nhap();
                p.Add(employee);
            }

            return p;
        }


        static void Output(List<IPerson> p)
        {
            int studentCount = 0;
            int teacherCount = 0;
            int employeeCount = 0;

            Console.WriteLine("Student:");
            foreach (var person in p)
            {
                if (person is Student student)
                {
                    studentCount++;
                    Console.WriteLine(student.GetInfo());
                }
            }
            Console.WriteLine($"Tong hoc sinh: {studentCount}");

            Console.WriteLine("Teacher:");
            foreach (var person in p)
            {
                if (person is Teacher teacher)
                {
                    teacherCount++;
                    Console.WriteLine(teacher.GetInfo());
                }
            }
            Console.WriteLine($"Tong giao vien: {teacherCount}");

            Console.WriteLine("Employee:");
            foreach (var person in p)
            {
                if (person is Employee employee)
                {
                    employeeCount++;
                    Console.WriteLine(employee.GetInfo());
                }
            }
            Console.WriteLine($"Tong nhan vien: {employeeCount}");
        }

        static void Main(string[] args)
        {
            TaxData taxData = new TaxData();
            List<IPerson> p = Init(taxData);
            Output(p);
        }
    }
}
