using System;

namespace Bai_3
{
    public class Program
    {
        static void TypeOfPerson()
        {
            Console.WriteLine("======Type of person=====");
            Console.WriteLine("1. Student");
            Console.WriteLine("2. Teacher");
            Console.WriteLine("3. Employee");
        }
        static void Init(List<Person> list, TaxData tax)
        {
            //hardcode
            list.Add(new Student("P01", "Nguyen Van A", 15, "ABC", "a1"));
            list.Add(new Student("P02", "Nguyen Van A", 15, "ABC", "a1"));
            list.Add(new Student("P03", "Nguyen Van A", 15, "ABC", "a2"));
            list.Add(new Student("P04", "Nguyen Van A", 15, "ABC", "a2"));
            list.Add(new Teacher("P05", "Nguyen Van B", 18, 10000000, tax, "ABC"));
            list.Add(new Teacher("P06", "Nguyen Van C", 20, 20000000, tax, "ABC"));
            list.Add(new Teacher("P07", "Nguyen Van A", 15, 10000000, tax, "ABC"));
            list.Add(new Employee("P08", "Nguyen Van B", 18, 10000000, tax, "HAT", "Team Leader"));
            list.Add(new Employee("P09", "Nguyen Van C", 20, 20000000, tax, "HAT", "CEO"));


            Console.Write("Number of person: ");
            int n = int.Parse(Console.ReadLine());
            for(int i=0; i<n; i++)
            {
                TypeOfPerson();
                Console.Write("Choose: ");
                int choose = int.Parse(Console.ReadLine());
                switch(choose)
                {
                    case 1:
                        Student st = new Student();
                        st.Init();
                        list.Add(st);
                        break;
                    case 2:
                        Teacher t = new Teacher();
                        t.Init();
                        list.Add(t);
                        break;
                    case 3:
                        Employee e = new Employee();
                        e.Init();
                        list.Add(e);
                        break;
                    
                }
            }

        }
        static void Output(List<Person> list)
        {
            int isStudent = 0, isTeacher = 0, isEmployee = 0;
            List<Student> stList = list.Where(e => e is Student).Cast<Student>().ToList();
            List<Teacher> tList = new List<Teacher>();
            List<Employee> eList = new List<Employee>();
            foreach (Person p in list)
            {
                if(p is Student st)
                {
                    //isStudent++;
                    //stList.Add(st);

                }else if(p is Teacher t)
                {
                    isTeacher++;
                    tList.Add(t);
                }else if(p is Employee e)
                { 
                    isEmployee++;
                    eList.Add(e);
                }
            }

            Console.WriteLine("\nStudent: "+stList.Count);
            Student.Title();
            foreach(Student st in stList)
            {
                st.GetInfo();
            }
            Console.WriteLine("\nTeacher: " + isTeacher);
            Teacher.Title();
            foreach (Teacher t in tList)
            {
                t.GetInfo();
            }
            Console.WriteLine("\nEmployee: " + isEmployee);
            Employee.Title();
            foreach (Employee e in eList)
            {
                e.GetInfo();
            }
        }

        public static void Main(string[] args)
        {
            List<Person> list = new List<Person>();
            TaxData tax = new TaxData();
            Init(list, tax);
            Output(list);
            
        }
    }
}
