using System;
using System.Collections;


namespace ArrayListCoBan
{

    public class Person
    {
        private string name;
        private int age;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public Person(string Name, int Age)
        {
            this.Name = Name;
            this.Age = Age;
        }

        public override string ToString()
        {
            return "Name: " + name + " | Age: " + age;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ArrayList MyArray = new ArrayList();

            MyArray.Add(new Person("Nguyen Van A", 22));
            MyArray.Add(new Person("Nguyen Van B", 20));
            MyArray.Add(new Person("Nguyen Van C", 25));
            MyArray.Add(new Person("Nguyen Van D", 27));

            Console.WriteLine("Danh sach person:");
            foreach (Person item in MyArray)
            {
                Console.WriteLine(item.ToString());
            }

            MyArray.Sort(new SortPersons());

            Console.WriteLine();
            Console.WriteLine("Danh sach Person da duoc sap xep theo tuoi tang dan: ");
            foreach (Person item in MyArray)
            {
                Console.WriteLine(item.ToString());
            }

            Console.ReadLine();
        }

        public class SortPersons : IComparer
        {
            public int Compare(object x, object y)
            {

                Person p1 = x as Person;
                Person p2 = y as Person;

                if (p1 == null || p2 == null)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    if (p1.Age > p2.Age)
                    {
                        return 1;
                    }
                    else if (p1.Age == p2.Age)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }
    }
}
