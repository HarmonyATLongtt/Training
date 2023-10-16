using System;

namespace Bai_3
{
    public class Program
    {

        static void Output(List<Person> list)
        {
            int isStudent = 0, isTeacher = 0, isEmployee = 0;
            List<Student> stList = new List<Student>();
            List<Teacher> tList = new List<Teacher>();
            List<Employee> eList = new List<Employee>();
            foreach (Person p in list)
            {
                if(p is Student st)
                {
                    isStudent++;
                    stList.Add(st);
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

            Console.WriteLine("\nStudent: "+isStudent);
            foreach(Student st in stList)
            {
                Console.WriteLine(st.GetInfo());
            }
            Console.WriteLine("\nTeacher: " + isTeacher);
            foreach (Teacher t in tList)
            {
                Console.WriteLine(t.GetInfo());
            }
            Console.WriteLine("\nEmployee: " + isEmployee);
            foreach (Employee e in eList)
            {
                Console.WriteLine(e.GetInfo());
            }
        }

        public static void Main(string[] args)
        {
            List<Person> list = new List<Person>();
            TaxData tax = new TaxData();
            list.Add(new Student("P01", "Nguyen Van A", 15, "ABC", "a1"));
            list.Add(new Student("P02", "Nguyen Van A", 15, "ABC", "a1"));
            list.Add(new Student("P03", "Nguyen Van A", 15, "ABC", "a2"));
            list.Add(new Student("P04", "Nguyen Van A", 15, "ABC", "a2"));
            list.Add(new Teacher("P05", "Nguyen Van B", 18, 10000000, tax, "ABC"));
            list.Add(new Teacher("P06", "Nguyen Van C", 20, 20000000, tax, "ABC"));
            list.Add(new Teacher("P07", "Nguyen Van A", 15, 10000000, tax, "ABC"));
            list.Add(new Employee("P08", "Nguyen Van B", 18, 10000000, tax, "HAT", "HR"));
            list.Add(new Employee("P09", "Nguyen Van C", 20, 20000000, tax, "HAT", "CEO"));
            Output(list);
        }
    }
}
