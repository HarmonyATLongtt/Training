using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP
{
    public class Person : IPerson
    {
        public int Id;
        public string Name;
        public int Age;
        public float Income;
        public float TaxCoe;

        public Person()
        { }

        public Person(int id, string name, int age, float income)
        {
            TaxData taxData = new TaxData();

            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person p1, Person p2)
        {
            if (p1.Id == p2.Id && p1.Name == p2.Name)
            {
                return true;
            }
            return false;
        }

        public double GetTax()
        {
            return Income - TaxCoe * Income;
        }

        public void GetInfo()
        {
            Console.WriteLine("ID:{0} co ten la: {1},thue phai dong: {2}", this.Id, this.Name, Math.Round(this.GetTax(), 2));
        }
    }

    public class Program
    {
        public static List<Person> Init()
        {
            Person p1 = new Person(1, "Duc", 17, 10000000);
            Person p2 = new Person(2, "Anh", 21, 8000000);
            Person p3 = new Person(3, "Vu", 28, 1600000);
            Person p4 = new Person(4, "Quy", 32, 2300000);

            //Person p1 = new Person();
            //Person p2 = new Person();
            //Person p3 = new Person();
            //Person p4 = new Person();

            //p1.Id = 1;
            //p1.Name = "Duc";
            //p1.Age = 18;
            //p1.Income = 100.3f;
            //p1.TaxCoe = 0.1f;

            //p2.Id = 2;
            //p2.Name = "Vuong";
            //p2.Age = 21;
            //p2.Income = 120.5f;
            //p2.TaxCoe = 0.1f;

            //p3.Id = 3;
            //p3.Name = "Phong";
            //p3.Age = 21;
            //p3.Income = 200.6f;
            //p3.TaxCoe = 0.1f;

            //p4.Id = 4;
            //p4.Name = "Hanh";
            //p4.Age = 22;
            //p4.Income = 115.5f;
            //p4.TaxCoe = 0.1f;

            List<Person> list = new List<Person>();
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);
            list.Add(p4);

            return list;
        }

        public static void Output(List<Person> list)
        {
            foreach (Person p in list)
            {
                p.GetInfo();
                //Console.WriteLine("ID:{0} co ten la: {1},thue phai dong: {2}", p.Id, p.Name, Math.Round(p.GetTax(),2));
            }
        }

        private static void Main(string[] args)
        {
            Output(Init());
            List<Student> listStudent = new List<Student>();
            List<Teacher> listTeacher = new List<Teacher>();
            List<Employee> listEmployee = new List<Employee>();

            Student s1 = new Student("10A2", "Gia Binh 1", 1, "Duc", 18);
            Teacher t1 = new Teacher("Gia Binh 1", 2, "Phu", 35, 1200000);
            Employee e1 = new Employee("Harmony At", "Intern", 3, "Duc", 31, 1500000);

            Student s2 = new Student("11A2", "Thai Bao", 4, "Quy", 14);
            Teacher t2 = new Teacher("Gia Binh 1", 5, "Phuong", 32, 1100000);
            Employee e2 = new Employee("Harmony At", "Fresher", 6, "Vuong", 29, 1200000);

            listStudent.Add(s1);
            listStudent.Add(s2);
            listTeacher.Add(t1);
            listTeacher.Add(t2);
            listEmployee.Add(e1);
            listEmployee.Add(e2);

            Console.WriteLine("Student: {0}", listStudent.Count());
            foreach (Student s in listStudent)
            {
                Console.WriteLine("{0}_{1}_{3}_{4}", s.Id, s.Name, s.Age, s.School, s.Class);
            }

            Console.WriteLine("Teacher: {0}", listTeacher.Count());
            foreach (Teacher t in listTeacher)
            {
                Console.WriteLine("{0}_{1}_{2}_{3}_{4}_{5}", t.Id, t.Name, t.Age, t.School, t.Income, t.TaxCoe);
            }

            Console.WriteLine("Employee: {0}", listEmployee.Count());
            foreach (Employee e in listEmployee)
            {
                Console.WriteLine("{0}_{1}_{2}_{3}_{4}_{5}", e.Id, e.Name, e.Age, e.Company, e.Income, e.TaxCoe);
            }
        }
    }
}